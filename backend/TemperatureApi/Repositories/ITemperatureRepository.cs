using TemperatureApi.Models;

namespace TemperatureApi.Repositories;

/// <summary>
/// Tầng truy cập dữ liệu cho nhiệt độ. Che giấu chi tiết EF Core khỏi tầng Service.
/// </summary>
public interface ITemperatureRepository
{
    Task<TemperatureLog> AddAsync(TemperatureLog log);
    Task<TemperatureLog?> GetLatestAsync();
    Task<List<TemperatureLog>> GetByRangeAsync(DateTime fromUtc, DateTime toUtc);
    Task<List<TemperatureLog>> GetRecentAsync(int count);
}
