using System.ComponentModel.DataAnnotations;

namespace TemperatureApi.Dtos;

/// <summary>DTO trả về cấu hình cảnh báo.</summary>
public class AlertSettingDto
{
    public int Id { get; set; }
    public double MaxTemperature { get; set; }
    public bool IsActive { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>DTO dùng để cập nhật cấu hình cảnh báo qua PUT /api/alert.</summary>
public class UpdateAlertSettingDto
{
    [Range(0, 100, ErrorMessage = "Ngưỡng nhiệt độ phải trong khoảng 0..100°C.")]
    public double MaxTemperature { get; set; }

    public bool IsActive { get; set; }
}

/// <summary>DTO một dòng nhật ký cảnh báo.</summary>
public class AlertLogDto
{
    public int Id { get; set; }
    public double Temperature { get; set; }
    public double MaxTemperature { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
