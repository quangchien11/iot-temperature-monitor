using Guna.UI2.WinForms;
using TemperatureMonitor.WinForms.Controls;
using TemperatureMonitor.WinForms.Services;

namespace TemperatureMonitor.WinForms.Forms;

/// <summary>
/// Màn hình chính: hiển thị nhiệt độ thời gian thực, biểu đồ, lịch sử,
/// và nút mở cấu hình cảnh báo. Tự làm mới mỗi 5 giây.
/// Giao diện hiện đại dùng Guna.UI2 (card bo góc, đổ bóng, gradient).
/// </summary>
public class FormDashboard : Form
{
    // ----- Bảng màu chủ đạo -----
    private static readonly Color Primary = Color.FromArgb(22, 119, 255);
    private static readonly Color Accent = Color.FromArgb(54, 207, 201);
    private static readonly Color BgColor = Color.FromArgb(240, 244, 251);
    private static readonly Color CardBg = Color.White;
    private static readonly Color TextMuted = Color.FromArgb(120, 134, 156);
    private static readonly Color Danger = Color.FromArgb(255, 77, 79);
    private static readonly Color Success = Color.FromArgb(82, 196, 26);

    private readonly ApiService _api = new();
    private readonly System.Windows.Forms.Timer _timer = new() { Interval = 5000 };

    // Controls
    private Label _lblTemp = null!, _lblHumid = null!, _lblMax = null!, _lblMin = null!,
                  _lblStatus = null!, _lblUpdated = null!, _lblConn = null!;
    private Guna2ComboBox _cboPeriod = null!;
    private LineChartControl _chart = null!;
    private Guna2DataGridView _grid = null!;
    private SplitContainer _split = null!;

    public FormDashboard()
    {
        Text = "IoT · Giám sát Nhiệt độ — Dashboard";
        Width = 1040; Height = 760;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = BgColor;
        Font = new Font("Segoe UI", 9);
        DoubleBuffered = true;

        BuildUi();

        _timer.Tick += async (_, _) => await RefreshAsync();
        Load += async (_, _) =>
        {
            // Gán SplitterDistance ở đây (sau khi form đã có kích thước thật)
            // để tránh InvalidOperationException khi control còn nhỏ lúc khởi tạo.
            SetSplitterDistanceSafe(330);
            await RefreshAsync();
            _timer.Start();
        };
    }

    private void BuildUi()
    {
        // ---- Thanh tiêu đề (gradient) ----
        var header = new Guna2GradientPanel
        {
            Dock = DockStyle.Top,
            Height = 64,
            FillColor = Primary,
            FillColor2 = Accent
        };
        header.Controls.Add(new Label
        {
            Text = "  ⚡  HỆ THỐNG GIÁM SÁT & CẢNH BÁO NHIỆT ĐỘ",
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 15, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(16, 16),
            BackColor = Color.Transparent
        });
        _lblConn = new Label
        {
            Text = "● Đang kết nối",
            ForeColor = Color.White,
            AutoSize = true,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Location = new Point(860, 22),
            BackColor = Color.Transparent,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        header.Controls.Add(_lblConn);
        Controls.Add(header);

        // ---- Hàng thẻ số liệu ----
        var cards = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 134,
            Padding = new Padding(14, 14, 14, 6),
            BackColor = BgColor
        };
        _lblTemp = MakeCard(cards, "🌡", "Nhiệt độ hiện tại", "-- °C", Color.FromArgb(250, 84, 28));
        _lblHumid = MakeCard(cards, "💧", "Độ ẩm hiện tại", "-- %", Primary);
        _lblMax = MakeCard(cards, "▲", "Cao nhất hôm nay", "-- °C", Color.FromArgb(250, 140, 22));
        _lblMin = MakeCard(cards, "▼", "Thấp nhất hôm nay", "-- °C", Color.FromArgb(19, 194, 194));
        _lblStatus = MakeCard(cards, "✓", "Trạng thái", "BÌNH THƯỜNG", Success);
        Controls.Add(cards);

        // ---- Thanh công cụ ----
        var toolbar = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = BgColor, Padding = new Padding(14, 0, 14, 0) };
        toolbar.Controls.Add(new Label
        {
            Text = "Biểu đồ theo:", AutoSize = true, Location = new Point(16, 16),
            ForeColor = TextMuted, Font = new Font("Segoe UI", 9.5f)
        });
        _cboPeriod = new Guna2ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Location = new Point(108, 11),
            Width = 130, Height = 30,
            BorderRadius = 8,
            BorderColor = Color.FromArgb(220, 226, 236),
            FillColor = CardBg,
            Font = new Font("Segoe UI", 9.5f)
        };
        _cboPeriod.Items.AddRange(new object[] { "Giờ", "Ngày", "Tuần" });
        _cboPeriod.SelectedIndex = 0;
        _cboPeriod.SelectedIndexChanged += async (_, _) => await LoadChartAsync();
        toolbar.Controls.Add(_cboPeriod);

        var btnSetting = MakeButton("⚙  Cấu hình cảnh báo", new Point(258, 10), 178, Primary);
        btnSetting.Click += (_, _) => { using var f = new FormSetting(_api); f.ShowDialog(this); };
        toolbar.Controls.Add(btnSetting);

        var btnRefresh = MakeButton("↻  Làm mới", new Point(448, 10), 120, Color.FromArgb(99, 110, 130));
        btnRefresh.Click += async (_, _) => await RefreshAsync();
        toolbar.Controls.Add(btnRefresh);

        _lblUpdated = new Label
        {
            Text = "", AutoSize = true, Location = new Point(588, 16),
            ForeColor = TextMuted, Font = new Font("Segoe UI", 9)
        };
        toolbar.Controls.Add(_lblUpdated);
        Controls.Add(toolbar);

        // ---- Khu vực chính: biểu đồ + bảng ----
        _split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            SplitterWidth = 12,
            BackColor = BgColor,
            Panel1MinSize = 140,
            Panel2MinSize = 120
        };
        _split.Panel1.Padding = new Padding(14, 6, 14, 6);
        _split.Panel2.Padding = new Padding(14, 6, 14, 14);

        // Biểu đồ trong card bo góc
        var chartCard = MakeRoundedPanel();
        chartCard.Dock = DockStyle.Fill;
        chartCard.Padding = new Padding(8);
        _chart = new LineChartControl { Dock = DockStyle.Fill, BackColor = CardBg };
        chartCard.Controls.Add(_chart);
        _split.Panel1.Controls.Add(chartCard);

        // Bảng lịch sử trong card bo góc
        var gridCard = MakeRoundedPanel();
        gridCard.Dock = DockStyle.Fill;
        gridCard.Padding = new Padding(10);
        gridCard.Controls.Add(new Label
        {
            Text = "🕘  Lịch sử nhiệt độ gần nhất", Dock = DockStyle.Top,
            Font = new Font("Segoe UI", 11, FontStyle.Bold), Height = 28,
            ForeColor = Color.FromArgb(40, 52, 74)
        });
        _grid = BuildGrid();
        gridCard.Controls.Add(_grid);
        _grid.BringToFront();
        _split.Panel2.Controls.Add(gridCard);

        Controls.Add(_split);
        _split.BringToFront();
    }

    // Gán vị trí thanh chia an toàn: luôn nằm trong [Panel1MinSize, Height - Panel2MinSize].
    private void SetSplitterDistanceSafe(int distance)
    {
        int min = _split.Panel1MinSize;
        int max = _split.Height - _split.Panel2MinSize - _split.SplitterWidth;
        if (max > min)
            _split.SplitterDistance = Math.Clamp(distance, min, max);
    }

    // ----- Helpers giao diện -----

    // Tạo một panel trắng bo góc + đổ bóng nhẹ.
    private static Guna2Panel MakeRoundedPanel()
    {
        var panel = new Guna2Panel
        {
            FillColor = CardBg,
            BorderRadius = 14,
            BorderThickness = 0
        };
        panel.ShadowDecoration.Enabled = true;
        panel.ShadowDecoration.Depth = 6;
        panel.ShadowDecoration.Color = Color.FromArgb(180, 190, 210);
        panel.ShadowDecoration.Shadow = new Padding(4);
        return panel;
    }

    // Tạo nút bo góc màu đặc.
    private static Guna2Button MakeButton(string text, Point location, int width, Color color)
    {
        return new Guna2Button
        {
            Text = text,
            Location = location,
            Size = new Size(width, 30),
            BorderRadius = 8,
            FillColor = color,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            Cursor = Cursors.Hand,
            Animated = true
        };
    }

    // Tạo một "thẻ" số liệu (icon + tiêu đề + giá trị), trả về Label giá trị.
    private Label MakeCard(Control parent, string icon, string title, string value, Color color)
    {
        var card = new Guna2Panel
        {
            Width = 192, Height = 100,
            FillColor = CardBg,
            BorderRadius = 14,
            Margin = new Padding(8)
        };
        card.ShadowDecoration.Enabled = true;
        card.ShadowDecoration.Depth = 5;
        card.ShadowDecoration.Color = Color.FromArgb(185, 195, 214);
        card.ShadowDecoration.Shadow = new Padding(3);

        // Dải màu nhấn bên trái
        card.Controls.Add(new Guna2Panel
        {
            Width = 6, Height = 70, Location = new Point(0, 15),
            FillColor = color, BorderRadius = 3
        });

        card.Controls.Add(new Label
        {
            Text = icon, Font = new Font("Segoe UI Emoji", 16), AutoSize = true,
            Location = new Point(150, 12), ForeColor = color, BackColor = Color.Transparent
        });
        card.Controls.Add(new Label
        {
            Text = title, AutoSize = true, ForeColor = TextMuted,
            Font = new Font("Segoe UI", 9), Location = new Point(16, 16), BackColor = Color.Transparent
        });
        var lbl = new Label
        {
            Text = value, Font = new Font("Segoe UI", 19, FontStyle.Bold),
            ForeColor = color, AutoSize = true, Location = new Point(15, 44), BackColor = Color.Transparent
        };
        card.Controls.Add(lbl);

        parent.Controls.Add(card);
        return lbl;
    }

    // Tạo lưới dữ liệu Guna với theme hiện đại.
    private Guna2DataGridView BuildGrid()
    {
        var grid = new Guna2DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToResizeRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            RowHeadersVisible = false,
            BackgroundColor = CardBg,
            BorderStyle = BorderStyle.None,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
            ColumnHeadersHeight = 38,
            GridColor = Color.FromArgb(238, 242, 247),
            Font = new Font("Segoe UI", 9)
        };
        grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(230, 240, 255);
        grid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(40, 52, 74);
        grid.DefaultCellStyle.Padding = new Padding(4, 2, 4, 2);
        grid.RowTemplate.Height = 32;

        var theme = grid.ThemeStyle;
        theme.HeaderStyle.BackColor = Color.FromArgb(245, 248, 252);
        theme.HeaderStyle.ForeColor = Color.FromArgb(80, 94, 116);
        theme.HeaderStyle.Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold);
        theme.HeaderStyle.Height = 38;
        theme.RowsStyle.BackColor = CardBg;
        theme.RowsStyle.ForeColor = Color.FromArgb(50, 62, 84);
        theme.RowsStyle.Height = 32;
        theme.AlternatingRowsStyle.BackColor = Color.FromArgb(248, 250, 253);
        return grid;
    }

    // ---- Tải dữ liệu ----
    private async Task RefreshAsync()
    {
        try
        {
            var d = await _api.GetDashboardAsync();
            if (d != null)
            {
                _lblConn.Text = "● Đã kết nối";
                _lblConn.ForeColor = Color.White;
                _lblTemp.Text = $"{d.CurrentTemperature:0.0} °C";
                _lblHumid.Text = $"{d.CurrentHumidity:0.0} %";
                _lblMax.Text = $"{d.MaxTemperatureToday:0.0} °C";
                _lblMin.Text = $"{d.MinTemperatureToday:0.0} °C";
                _lblTemp.ForeColor = d.IsAlert ? Danger : Color.FromArgb(250, 84, 28);
                _lblStatus.Text = d.IsAlert ? "⚠ CẢNH BÁO!" : "BÌNH THƯỜNG";
                _lblStatus.ForeColor = d.IsAlert ? Danger : Success;
                _lblUpdated.Text = d.LastUpdated.HasValue
                    ? $"Cập nhật: {d.LastUpdated:HH:mm:ss dd/MM}" : "";
            }
            await LoadChartAsync();
            await LoadHistoryAsync();
        }
        catch
        {
            _lblConn.Text = "● Mất kết nối";
            _lblConn.ForeColor = Color.FromArgb(255, 220, 220);
        }
    }

    private async Task LoadChartAsync()
    {
        string period = _cboPeriod.SelectedIndex switch { 1 => "daily", 2 => "weekly", _ => "hourly" };
        var data = await _api.GetStatisticsAsync(period);
        if (data == null) return;
        _chart.Title = $"Biểu đồ nhiệt độ theo {_cboPeriod.Text.ToLower()}";
        _chart.SetData(
            data.Select(x => x.Label).ToArray(),
            data.Select(x => x.AvgTemperature).ToArray(),
            data.Select(x => x.MinTemperature).ToArray(),
            data.Select(x => x.MaxTemperature).ToArray());
    }

    private async Task LoadHistoryAsync()
    {
        var data = await _api.GetHistoryAsync(50);
        if (data == null) return;
        _grid.DataSource = data.Select(x => new
        {
            Mã = x.Id,
            NhiệtĐộ = $"{x.Temperature:0.0} °C",
            ĐộẨm = $"{x.Humidity:0.0} %",
            TrạngThái = x.IsAlert ? "⚠ Cảnh báo" : "Bình thường",
            ThờiGian = x.CreatedAt.ToString("HH:mm:ss dd/MM/yyyy")
        }).ToList();
    }
}
