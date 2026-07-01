namespace TemperatureApi.Models;

/// <summary>
/// Cấu hình cảnh báo. Bảng này chỉ giữ 1 dòng duy nhất (Id = 1) đại diện cho
/// cấu hình hiện hành của hệ thống.
/// </summary>
public class AlertSetting
{
    public int Id { get; set; }

    /// <summary>Ngưỡng nhiệt độ tối đa. Vượt quá giá trị này sẽ kích hoạt cảnh báo.</summary>
    public double MaxTemperature { get; set; }

    /// <summary>Bật/tắt toàn bộ chức năng cảnh báo.</summary>
    public bool IsActive { get; set; }

    /// <summary>Thời điểm cập nhật cấu hình gần nhất.</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
