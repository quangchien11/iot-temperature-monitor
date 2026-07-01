using System.Globalization;
using TemperatureApi.Dtos;
using TemperatureApi.Models;
using TemperatureApi.Repositories;

namespace TemperatureApi.Services;

public class TemperatureService : ITemperatureService
{
    private readonly ITemperatureRepository _tempRepo;
    private readonly IAlertRepository _alertRepo;

    public TemperatureService(ITemperatureRepository tempRepo, IAlertRepository alertRepo)
    {
        _tempRepo = tempRepo;
        _alertRepo = alertRepo;
    }

    /// <summary>
    /// Luồng xử lý chính khi Arduino gửi dữ liệu:
    /// 1. Lấy cấu hình cảnh báo.
    /// 2. So sánh nhiệt độ với ngưỡng (chỉ khi cảnh báo đang bật).
    /// 3. Lưu bản ghi nhiệt độ (kèm cờ IsAlert).
    /// 4. Nếu vượt ngưỡng -> ghi thêm nhật ký cảnh báo.
    /// 5. Trả về trạng thái để Arduino bật/tắt LED + Buzzer.
    /// </summary>
    public async Task<TemperatureResponseDto> SaveAsync(TemperatureInputDto input)
    {
        var setting = await _alertRepo.GetSettingAsync();
        bool isAlert = setting.IsActive && input.Temperature > setting.MaxTemperature;

        var log = new TemperatureLog
        {
            Temperature = input.Temperature,
            Humidity = input.Humidity,
            IsAlert = isAlert,
            CreatedAt = DateTime.Now
        };
        await _tempRepo.AddAsync(log);

        if (isAlert)
        {
            await _alertRepo.AddLogAsync(new AlertLog
            {
                Temperature = input.Temperature,
                MaxTemperature = setting.MaxTemperature,
                Message = $"Nhiệt độ {input.Temperature:0.0}°C vượt ngưỡng {setting.MaxTemperature:0.0}°C",
                CreatedAt = DateTime.Now
            });
        }

        return new TemperatureResponseDto
        {
            Saved = true,
            IsAlert = isAlert,
            MaxTemperature = setting.MaxTemperature,
            Message = isAlert
                ? "ALERT: Nhiệt độ vượt ngưỡng! Bật LED và Buzzer."
                : "OK: Nhiệt độ bình thường."
        };
    }

    public async Task<TemperatureDto?> GetLatestAsync()
    {
        var log = await _tempRepo.GetLatestAsync();
        return log == null ? null : ToDto(log);
    }

    public async Task<DashboardDto> GetDashboardAsync()
    {
        var setting = await _alertRepo.GetSettingAsync();
        var latest = await _tempRepo.GetLatestAsync();

        var startOfDay = DateTime.Today;
        var endOfDay = startOfDay.AddDays(1).AddTicks(-1);
        var today = await _tempRepo.GetByRangeAsync(startOfDay, endOfDay);

        return new DashboardDto
        {
            CurrentTemperature = latest?.Temperature ?? 0,
            CurrentHumidity = latest?.Humidity ?? 0,
            IsAlert = latest?.IsAlert ?? false,
            MaxTemperatureToday = today.Count > 0 ? today.Max(x => x.Temperature) : 0,
            MinTemperatureToday = today.Count > 0 ? today.Min(x => x.Temperature) : 0,
            MaxTemperatureThreshold = setting.MaxTemperature,
            AlertEnabled = setting.IsActive,
            LastUpdated = latest?.CreatedAt
        };
    }

    /// <summary>Thống kê trung bình/min/max theo từng giờ trong 24 giờ gần nhất.</summary>
    public async Task<List<TemperatureStatisticDto>> GetHourlyAsync()
    {
        var to = DateTime.Now;
        var from = to.AddHours(-24);
        var data = await _tempRepo.GetByRangeAsync(from, to);

        return data
            .GroupBy(x => new DateTime(x.CreatedAt.Year, x.CreatedAt.Month, x.CreatedAt.Day, x.CreatedAt.Hour, 0, 0))
            .OrderBy(g => g.Key)
            .Select(g => Aggregate(g.Key.ToString("HH:00", CultureInfo.InvariantCulture), g))
            .ToList();
    }

    /// <summary>Thống kê theo từng ngày trong 7 ngày gần nhất.</summary>
    public async Task<List<TemperatureStatisticDto>> GetDailyAsync()
    {
        var to = DateTime.Now;
        var from = DateTime.Today.AddDays(-6);
        var data = await _tempRepo.GetByRangeAsync(from, to);

        return data
            .GroupBy(x => x.CreatedAt.Date)
            .OrderBy(g => g.Key)
            .Select(g => Aggregate(g.Key.ToString("dd/MM", CultureInfo.InvariantCulture), g))
            .ToList();
    }

    /// <summary>Thống kê theo từng tuần (ISO week) trong 8 tuần gần nhất.</summary>
    public async Task<List<TemperatureStatisticDto>> GetWeeklyAsync()
    {
        var to = DateTime.Now;
        var from = DateTime.Today.AddDays(-7 * 8);
        var data = await _tempRepo.GetByRangeAsync(from, to);

        return data
            .GroupBy(x => ISOWeek.GetWeekOfYear(x.CreatedAt))
            .OrderBy(g => g.Key)
            .Select(g => Aggregate($"Tuần {g.Key}", g))
            .ToList();
    }

    public async Task<List<TemperatureDto>> GetHistoryAsync(int count)
    {
        var data = await _tempRepo.GetRecentAsync(count);
        return data.Select(ToDto).ToList();
    }

    // ----- Helpers -----

    private static TemperatureStatisticDto Aggregate(string label, IEnumerable<TemperatureLog> group)
    {
        var list = group.ToList();
        return new TemperatureStatisticDto
        {
            Label = label,
            AvgTemperature = Math.Round(list.Average(x => x.Temperature), 1),
            MinTemperature = Math.Round(list.Min(x => x.Temperature), 1),
            MaxTemperature = Math.Round(list.Max(x => x.Temperature), 1),
            AvgHumidity = Math.Round(list.Average(x => x.Humidity), 1),
            Count = list.Count
        };
    }

    private static TemperatureDto ToDto(TemperatureLog x) => new()
    {
        Id = x.Id,
        Temperature = x.Temperature,
        Humidity = x.Humidity,
        IsAlert = x.IsAlert,
        CreatedAt = x.CreatedAt
    };
}
