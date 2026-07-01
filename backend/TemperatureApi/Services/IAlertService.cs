using TemperatureApi.Dtos;

namespace TemperatureApi.Services;

public interface IAlertService
{
    Task<AlertSettingDto> GetAsync();
    Task<AlertSettingDto> UpdateAsync(UpdateAlertSettingDto dto);
    Task<List<AlertLogDto>> GetRecentLogsAsync(int count);
}
