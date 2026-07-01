using Microsoft.EntityFrameworkCore;
using TemperatureApi.Data;
using TemperatureApi.Models;

namespace TemperatureApi.Repositories;

public class AlertRepository : IAlertRepository
{
    private readonly AppDbContext _db;

    public AlertRepository(AppDbContext db) => _db = db;

    /// <summary>
    /// Luôn trả về cấu hình có Id = 1. Nếu chưa có (DB trống) thì tạo mặc định.
    /// </summary>
    public async Task<AlertSetting> GetSettingAsync()
    {
        var setting = await _db.AlertSettings.FirstOrDefaultAsync(x => x.Id == 1);
        if (setting == null)
        {
            setting = new AlertSetting { Id = 1, MaxTemperature = 35, IsActive = true, UpdatedAt = DateTime.Now };
            _db.AlertSettings.Add(setting);
            await _db.SaveChangesAsync();
        }
        return setting;
    }

    public async Task<AlertSetting> UpdateSettingAsync(double maxTemperature, bool isActive)
    {
        var setting = await GetSettingAsync();
        setting.MaxTemperature = maxTemperature;
        setting.IsActive = isActive;
        setting.UpdatedAt = DateTime.Now;
        await _db.SaveChangesAsync();
        return setting;
    }

    public async Task<AlertLog> AddLogAsync(AlertLog log)
    {
        _db.AlertLogs.Add(log);
        await _db.SaveChangesAsync();
        return log;
    }

    public Task<List<AlertLog>> GetRecentLogsAsync(int count) =>
        _db.AlertLogs
           .OrderByDescending(x => x.CreatedAt)
           .Take(count)
           .ToListAsync();
}
