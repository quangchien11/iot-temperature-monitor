using System.ComponentModel.DataAnnotations;

namespace TemperatureApi.Dtos;

/// <summary>Dữ liệu Arduino gửi lên qua POST /api/temperature.</summary>
public class TemperatureInputDto
{
    [Range(-40, 125, ErrorMessage = "Nhiệt độ nằm ngoài khoảng đo của cảm biến (-40..125°C).")]
    public double Temperature { get; set; }

    [Range(0, 100, ErrorMessage = "Độ ẩm phải trong khoảng 0..100%.")]
    public double Humidity { get; set; }
}

/// <summary>
/// Phản hồi cho Arduino sau khi POST dữ liệu. Arduino dựa vào IsAlert để
/// bật/tắt LED đỏ và Buzzer.
/// </summary>
public class TemperatureResponseDto
{
    public bool Saved { get; set; }
    public bool IsAlert { get; set; }
    public double MaxTemperature { get; set; }
    public string Message { get; set; } = string.Empty;
}

/// <summary>DTO trả về một bản ghi nhiệt độ cho client (Web, WinForm, Mobile).</summary>
public class TemperatureDto
{
    public int Id { get; set; }
    public double Temperature { get; set; }
    public double Humidity { get; set; }
    public bool IsAlert { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>Một điểm thống kê theo giờ/ngày/tuần.</summary>
public class TemperatureStatisticDto
{
    /// <summary>Nhãn hiển thị: "14:00", "29/06", "Tuần 26"...</summary>
    public string Label { get; set; } = string.Empty;

    public double AvgTemperature { get; set; }
    public double MinTemperature { get; set; }
    public double MaxTemperature { get; set; }
    public double AvgHumidity { get; set; }
    public int Count { get; set; }
}

/// <summary>Tổng hợp số liệu cho màn hình Dashboard.</summary>
public class DashboardDto
{
    public double CurrentTemperature { get; set; }
    public double CurrentHumidity { get; set; }
    public bool IsAlert { get; set; }
    public double MaxTemperatureToday { get; set; }
    public double MinTemperatureToday { get; set; }
    public double MaxTemperatureThreshold { get; set; }
    public bool AlertEnabled { get; set; }
    public DateTime? LastUpdated { get; set; }
}
