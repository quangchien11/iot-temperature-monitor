namespace TemperatureMonitor.WinForms.Models;

// Các lớp này khớp với DTO trả về từ ASP.NET Core API.

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

public class TemperatureDto
{
    public int Id { get; set; }
    public double Temperature { get; set; }
    public double Humidity { get; set; }
    public bool IsAlert { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class TemperatureStatisticDto
{
    public string Label { get; set; } = "";
    public double AvgTemperature { get; set; }
    public double MinTemperature { get; set; }
    public double MaxTemperature { get; set; }
    public double AvgHumidity { get; set; }
    public int Count { get; set; }
}

public class AlertSettingDto
{
    public int Id { get; set; }
    public double MaxTemperature { get; set; }
    public bool IsActive { get; set; }
    public DateTime UpdatedAt { get; set; }
}
