using Guna.UI2.WinForms;
using TemperatureMonitor.WinForms.Services;

namespace TemperatureMonitor.WinForms.Forms;

/// <summary>
/// Cửa sổ cấu hình cảnh báo: đặt ngưỡng nhiệt độ tối đa và bật/tắt cảnh báo.
/// Giao diện hiện đại dùng Guna.UI2.
/// </summary>
public class FormSetting : Form
{
    private static readonly Color Primary = Color.FromArgb(22, 119, 255);
    private static readonly Color Accent = Color.FromArgb(54, 207, 201);
    private static readonly Color CardBg = Color.White;
    private static readonly Color TextMuted = Color.FromArgb(120, 134, 156);

    private readonly ApiService _api;
    private Guna2NumericUpDown _numMax = null!;
    private Guna2ToggleSwitch _tglActive = null!;
    private Label _lblMsg = null!;

    public FormSetting(ApiService api)
    {
        _api = api;
        Text = "Cấu hình cảnh báo";
        Width = 420; Height = 320;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false; MinimizeBox = false;
        BackColor = Color.FromArgb(240, 244, 251);
        Font = new Font("Segoe UI", 10);

        BuildUi();
        Load += async (_, _) => await LoadAsync();
    }

    private void BuildUi()
    {
        // Header gradient
        var header = new Guna2GradientPanel
        {
            Dock = DockStyle.Top, Height = 56,
            FillColor = Primary, FillColor2 = Accent
        };
        header.Controls.Add(new Label
        {
            Text = "  ⚙  Cấu hình cảnh báo", ForeColor = Color.White,
            Font = new Font("Segoe UI", 13, FontStyle.Bold), AutoSize = true,
            Location = new Point(14, 14), BackColor = Color.Transparent
        });
        Controls.Add(header);

        // Card nội dung
        var card = new Guna2Panel
        {
            FillColor = CardBg, BorderRadius = 14,
            Location = new Point(20, 76), Size = new Size(364, 196)
        };
        card.ShadowDecoration.Enabled = true;
        card.ShadowDecoration.Depth = 6;
        card.ShadowDecoration.Color = Color.FromArgb(180, 190, 210);
        card.ShadowDecoration.Shadow = new Padding(4);
        Controls.Add(card);

        card.Controls.Add(new Label
        {
            Text = "Ngưỡng nhiệt độ tối đa (°C)", AutoSize = true,
            Location = new Point(22, 20), ForeColor = TextMuted, BackColor = Color.Transparent
        });
        _numMax = new Guna2NumericUpDown
        {
            Location = new Point(22, 46), Width = 150, Height = 38,
            DecimalPlaces = 1, Minimum = 0, Maximum = 100, Increment = 0.5M,
            BorderRadius = 8, BorderColor = Color.FromArgb(220, 226, 236),
            FillColor = Color.White, Font = new Font("Segoe UI", 11)
        };
        card.Controls.Add(_numMax);

        card.Controls.Add(new Label
        {
            Text = "Bật cảnh báo", AutoSize = true,
            Location = new Point(22, 104), ForeColor = Color.FromArgb(50, 62, 84),
            Font = new Font("Segoe UI", 10), BackColor = Color.Transparent
        });
        _tglActive = new Guna2ToggleSwitch
        {
            Location = new Point(140, 100), Size = new Size(46, 24),
            CheckedState = { FillColor = Color.FromArgb(82, 196, 26) }
        };
        card.Controls.Add(_tglActive);

        var btnSave = new Guna2Button
        {
            Text = "💾  Lưu cấu hình", Location = new Point(22, 144), Size = new Size(160, 38),
            BorderRadius = 9, FillColor = Primary, ForeColor = Color.White,
            Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand, Animated = true
        };
        btnSave.Click += async (_, _) => await SaveAsync();
        card.Controls.Add(btnSave);

        var btnClose = new Guna2Button
        {
            Text = "Đóng", Location = new Point(194, 144), Size = new Size(148, 38),
            BorderRadius = 9, FillColor = Color.FromArgb(238, 242, 247),
            ForeColor = Color.FromArgb(80, 94, 116), Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        btnClose.Click += (_, _) => Close();
        card.Controls.Add(btnClose);

        _lblMsg = new Label
        {
            AutoSize = true, Location = new Point(20, 278), ForeColor = Color.Green,
            Font = new Font("Segoe UI", 9.5f), BackColor = Color.Transparent
        };
        Controls.Add(_lblMsg);
    }

    private async Task LoadAsync()
    {
        try
        {
            var s = await _api.GetAlertAsync();
            if (s != null)
            {
                _numMax.Value = (decimal)s.MaxTemperature;
                _tglActive.Checked = s.IsActive;
            }
        }
        catch
        {
            _lblMsg.ForeColor = Color.Red;
            _lblMsg.Text = "Không tải được cấu hình (kiểm tra server).";
        }
    }

    private async Task SaveAsync()
    {
        try
        {
            bool ok = await _api.UpdateAlertAsync((double)_numMax.Value, _tglActive.Checked);
            _lblMsg.ForeColor = ok ? Color.FromArgb(46, 160, 67) : Color.Red;
            _lblMsg.Text = ok ? "✔ Đã lưu cấu hình thành công!" : "✖ Lưu thất bại.";
        }
        catch
        {
            _lblMsg.ForeColor = Color.Red;
            _lblMsg.Text = "✖ Lỗi kết nối server.";
        }
    }
}
