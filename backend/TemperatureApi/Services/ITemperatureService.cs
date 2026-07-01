using TemperatureApi.Dtos;

namespace TemperatureApi.Services;

public interface ITemperatureService
{
    /// <summary>Nhận dữ liệu từ Arduino, kiểm tra ngưỡng, lưu DB và trả trạng thái cảnh báo.</summary>
    Task<TemperatureResponseDto> SaveAsync(TemperatureInputDto input);

    Task<TemperatureDto?> GetLatestAsync();
    Task<DashboardDto> GetDashboardAsync();

    Task<List<TemperatureStatisticDto>> GetHourlyAsync();
    Task<List<TemperatureStatisticDto>> GetDailyAsync();
    Task<List<TemperatureStatisticDto>> GetWeeklyAsync();

    Task<List<TemperatureDto>> GetHistoryAsync(int count);
}
