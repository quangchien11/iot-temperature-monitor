using TemperatureMonitor.Maui.Models;

namespace TemperatureMonitor.Maui.Controls;

/// <summary>
/// Vẽ biểu đồ đường nhiệt độ (TB/Min/Max) lên GraphicsView bằng Microsoft.Maui.Graphics.
/// </summary>
public class LineChartDrawable : IDrawable
{
    public List<TemperatureStatisticDto> Data { get; set; } = new();

    public void Draw(ICanvas canvas, RectF rect)
    {
        canvas.FillColor = Colors.White;
        canvas.FillRectangle(rect);

        if (Data.Count == 0)
        {
            canvas.FontColor = Colors.Gray;
            canvas.DrawString("Chưa có dữ liệu...", rect, HorizontalAlignment.Center, VerticalAlignment.Center);
            return;
        }

        float marginL = 40, marginR = 15, marginT = 20, marginB = 30;
        var plot = new RectF(rect.Left + marginL, rect.Top + marginT,
            rect.Width - marginL - marginR, rect.Height - marginT - marginB);

        double yMin = Data.Min(d => d.MinTemperature);
        double yMax = Data.Max(d => d.MaxTemperature);
        if (Math.Abs(yMax - yMin) < 1) { yMax += 1; yMin -= 1; }
        yMin = Math.Floor(yMin - 1); yMax = Math.Ceiling(yMax + 1);

        // Lưới ngang + nhãn trục Y
        canvas.FontSize = 10;
        for (int i = 0; i <= 5; i++)
        {
            float y = plot.Bottom - i * plot.Height / 5;
            canvas.StrokeColor = Color.FromArgb("#EEEEEE");
            canvas.DrawLine(plot.Left, y, plot.Right, y);
            double val = yMin + (yMax - yMin) * i / 5;
            canvas.FontColor = Colors.Gray;
            canvas.DrawString(val.ToString("0.#"), plot.Left - 35, y - 7, 32, 14,
                HorizontalAlignment.Right, VerticalAlignment.Center);
        }

        PointF Map(int i, double v)
        {
            float x = Data.Count == 1 ? plot.Center.X
                : plot.Left + i * plot.Width / (Data.Count - 1);
            float y = plot.Bottom - (float)((v - yMin) / (yMax - yMin)) * plot.Height;
            return new PointF(x, y);
        }

        // Bảng màu đồng bộ với Web/WinForm
        var avgColor = Color.FromArgb("#1677FF");
        var maxColor = Color.FromArgb("#FF4D4F");
        var minColor = Color.FromArgb("#13C2C2");

        // Vùng tô mờ dưới đường trung bình
        FillArea(canvas, Data.Select(d => d.AvgTemperature).ToList(), Map, plot, avgColor);

        DrawLine(canvas, Data.Select(d => d.MaxTemperature).ToList(), Map, maxColor);
        DrawLine(canvas, Data.Select(d => d.AvgTemperature).ToList(), Map, avgColor);
        DrawLine(canvas, Data.Select(d => d.MinTemperature).ToList(), Map, minColor);

        // Nhãn trục X
        int step = Math.Max(1, Data.Count / 6);
        canvas.FontColor = Colors.Gray;
        for (int i = 0; i < Data.Count; i += step)
        {
            var p = Map(i, yMin);
            canvas.DrawString(Data[i].Label, p.X - 18, plot.Bottom + 4, 36, 14,
                HorizontalAlignment.Center, VerticalAlignment.Top);
        }
    }

    // Tô vùng mờ từ đường dữ liệu xuống đáy biểu đồ tạo cảm giác hiện đại.
    private static void FillArea(ICanvas canvas, List<double> series, Func<int, double, PointF> map, RectF plot, Color color)
    {
        if (series.Count < 2) return;
        var path = new PathF();
        var first = map(0, series[0]);
        path.MoveTo(first.X, first.Y);
        for (int i = 1; i < series.Count; i++)
        {
            var p = map(i, series[i]);
            path.LineTo(p.X, p.Y);
        }
        var last = map(series.Count - 1, series[^1]);
        path.LineTo(last.X, plot.Bottom);
        path.LineTo(first.X, plot.Bottom);
        path.Close();

        canvas.FillColor = color.WithAlpha(0.16f);
        canvas.FillPath(path);
    }

    private static void DrawLine(ICanvas canvas, List<double> series, Func<int, double, PointF> map, Color color)
    {
        canvas.StrokeColor = color;
        canvas.StrokeSize = 2;
        for (int i = 1; i < series.Count; i++)
        {
            var a = map(i - 1, series[i - 1]);
            var b = map(i, series[i]);
            canvas.DrawLine(a.X, a.Y, b.X, b.Y);
        }
        canvas.FillColor = color;
        for (int i = 0; i < series.Count; i++)
        {
            var p = map(i, series[i]);
            canvas.FillCircle(p.X, p.Y, 3);
        }
    }
}
