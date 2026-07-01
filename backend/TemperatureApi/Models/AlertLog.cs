namespace TemperatureApi.Models;

/// <summary>
/// Nhật ký cảnh báo - lưu lại mỗi lần hệ thống phát hiện nhiệt độ vượt ngưỡng.
/// Phục vụ thống kê và truy vết lịch sử cảnh báo.
/// </summary>
public class AlertLog
{
    public int Id { get; set; }

    /// <summary>Nhiệt độ tại thời điểm cảnh báo.</summary>
    public double Temperature { get; set; }

    /// <summary>Ngưỡng đang áp dụng khi cảnh báo xảy ra.</summary>
    public double MaxTemperature { get; set; }

    /// <summary>Nội dung mô tả cảnh báo.</summary>
    public string Message { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
