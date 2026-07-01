using System.ComponentModel.DataAnnotations;

namespace TemperatureApi.Models;

/// <summary>
/// Bản ghi nhiệt độ - mỗi lần Arduino gửi dữ liệu sẽ tạo 1 dòng trong bảng này.
/// </summary>
public class TemperatureLog
{
    public int Id { get; set; }

    /// <summary>Nhiệt độ đo được (°C)</summary>
    public double Temperature { get; set; }

    /// <summary>Độ ẩm đo được (%)</summary>
    public double Humidity { get; set; }

    /// <summary>Đánh dấu bản ghi này có vượt ngưỡng cảnh báo hay không.</summary>
    public bool IsAlert { get; set; }

    /// <summary>Thời điểm server nhận và lưu dữ liệu.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
