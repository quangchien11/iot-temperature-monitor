using TemperatureApi.Models;

namespace TemperatureApi.Repositories;

public interface IAlertRepository
{
    Task<AlertSetting> GetSettingAsync();
    Task<AlertSetting> UpdateSettingAsync(double maxTemperature, bool isActive);
    Task<AlertLog> AddLogAsync(AlertLog log);
    Task<List<AlertLog>> GetRecentLogsAsync(int count);
}
