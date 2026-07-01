using System.Drawing.Drawing2D;

namespace TemperatureMonitor.WinForms.Controls;

/// <summary>
/// Control biểu đồ đường đơn giản vẽ bằng GDI+ (không cần thư viện ngoài).
/// Dùng để hiển thị nhiệt độ TB/Min/Max theo giờ/ngày/tuần.
/// </summary>
public class LineChartControl : Control
{
    private string[] _labels = Array.Empty<string>();
    private double[] _avg = Array.Empty<double>();
    private double[] _min = Array.Empty<double>();
    private double[] _max = Array.Empty<double>();

    public string Title { get; set; } = "Biểu đồ nhiệt độ";

    public LineChartControl()
    {
        DoubleBuffered = true;
        BackColor = Color.White;
    }

    public void SetData(string[] labels, double[] avg, double[] min, double[] max)
    {
        _labels = labels; _avg = avg; _min = min; _max = max;
        Invalidate(); // yêu cầu vẽ lại
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        int marginL = 45, marginR = 20, marginT = 40, marginB = 40;
        var plot = new Rectangle(marginL, marginT,
            Width - marginL - marginR, Height - marginT - marginB);

        // Tiêu đề
        using var titleFont = new Font("Segoe UI", 11, FontStyle.Bold);
        g.DrawString(Title, titleFont, Brushes.Black, marginL, 8);

        if (_avg.Length == 0)
        {
            g.DrawString("Chưa có dữ liệu...", Font, Brushes.Gray, plot.Left + 10, plot.Top + 10);
            return;
        }

        // Tính min/max trục Y
        double yMin = _min.Min(), yMax = _max.Max();
        if (Math.Abs(yMax - yMin) < 1) { yMax += 1; yMin -= 1; }
        yMin = Math.Floor(yMin - 1); yMax = Math.Ceiling(yMax + 1);

        // Lưới + nhãn trục Y
        using var gridPen = new Pen(Color.FromArgb(230, 230, 230));
        using var axisPen = new Pen(Color.Gray);
        int yLines = 5;
        for (int i = 0; i <= yLines; i++)
        {
            int y = plot.Bottom - i * plot.Height / yLines;
            g.DrawLine(gridPen, plot.Left, y, plot.Right, y);
            double val = yMin + (yMax - yMin) * i / yLines;
            g.DrawString(val.ToString("0.#"), Font, Brushes.Gray, 5, y - 8);
        }
        g.DrawLine(axisPen, plot.Left, plot.Top, plot.Left, plot.Bottom);
        g.DrawLine(axisPen, plot.Left, plot.Bottom, plot.Right, plot.Bottom);

        // Hàm chuyển giá trị -> tọa độ điểm
        PointF ToPoint(int i, double val)
        {
            float x = plot.Left + (_avg.Length == 1 ? plot.Width / 2f
                        : i * plot.Width / (float)(_avg.Length - 1));
            float y = plot.Bottom - (float)((val - yMin) / (yMax - yMin) * plot.Height);
            return new PointF(x, y);
        }

        // Vùng tô gradient dưới đường trung bình tạo cảm giác hiện đại.
        DrawAreaFill(g, _avg, ToPoint, plot, Color.FromArgb(22, 119, 255));

        DrawSeries(g, _max, ToPoint, Color.FromArgb(255, 77, 79), "Cao nhất");
        DrawSeries(g, _avg, ToPoint, Color.FromArgb(22, 119, 255), "Trung bình");
        DrawSeries(g, _min, ToPoint, Color.FromArgb(19, 194, 194), "Thấp nhất");

        // Nhãn trục X (giãn cách để không chồng nhau)
        int step = Math.Max(1, _labels.Length / 8);
        for (int i = 0; i < _labels.Length; i += step)
        {
            var p = ToPoint(i, yMin);
            g.DrawString(_labels[i], Font, Brushes.Gray, p.X - 12, plot.Bottom + 5);
        }

        // Chú thích
        DrawLegend(g, plot.Right - 110, plot.Top + 5);
    }

    // Tô vùng gradient từ đường dữ liệu xuống đáy biểu đồ.
    private static void DrawAreaFill(Graphics g, double[] data, Func<int, double, PointF> map, Rectangle plot, Color color)
    {
        if (data.Length < 2) return;
        var pts = new PointF[data.Length + 2];
        for (int i = 0; i < data.Length; i++) pts[i] = map(i, data[i]);
        pts[data.Length] = new PointF(pts[data.Length - 1].X, plot.Bottom);
        pts[data.Length + 1] = new PointF(pts[0].X, plot.Bottom);

        using var brush = new LinearGradientBrush(
            new Rectangle(plot.Left, plot.Top, plot.Width, plot.Height),
            Color.FromArgb(70, color), Color.FromArgb(5, color), 90f);
        g.FillPolygon(brush, pts);
    }

    private static void DrawSeries(Graphics g, double[] data, Func<int, double, PointF> map, Color color, string name)
    {
        if (data.Length == 0) return;
        using var pen = new Pen(color, 2);
        var pts = new PointF[data.Length];
        for (int i = 0; i < data.Length; i++) pts[i] = map(i, data[i]);
        if (pts.Length > 1) g.DrawLines(pen, pts);
        using var brush = new SolidBrush(color);
        foreach (var p in pts) g.FillEllipse(brush, p.X - 3, p.Y - 3, 6, 6);
    }

    private void DrawLegend(Graphics g, int x, int y)
    {
        var items = new (Color, string)[]
        {
            (Color.FromArgb(255, 77, 79), "Cao nhất"),
            (Color.FromArgb(22, 119, 255), "TB"),
            (Color.FromArgb(19, 194, 194), "Thấp nhất")
        };
        foreach (var (c, name) in items)
        {
            using var b = new SolidBrush(c);
            g.FillRectangle(b, x, y, 12, 12);
            g.DrawString(name, Font, Brushes.Black, x + 16, y - 2);
            y += 18;
        }
    }
}
