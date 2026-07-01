using Microsoft.EntityFrameworkCore;
using TemperatureApi.Data;
using TemperatureApi.Models;

namespace TemperatureApi.Repositories;

public class TemperatureRepository : ITemperatureRepository
{
    private readonly AppDbContext _db;

    public TemperatureRepository(AppDbContext db) => _db = db;

    public async Task<TemperatureLog> AddAsync(TemperatureLog log)
    {
        _db.TemperatureLogs.Add(log);
        await _db.SaveChangesAsync();
        return log;
    }

    public Task<TemperatureLog?> GetLatestAsync() =>
        _db.TemperatureLogs
           .OrderByDescending(x => x.CreatedAt)
           .FirstOrDefaultAsync();

    public Task<List<TemperatureLog>> GetByRangeAsync(DateTime from, DateTime to) =>
        _db.TemperatureLogs
           .Where(x => x.CreatedAt >= from && x.CreatedAt <= to)
           .OrderBy(x => x.CreatedAt)
           .ToListAsync();

    public Task<List<TemperatureLog>> GetRecentAsync(int count) =>
        _db.TemperatureLogs
           .OrderByDescending(x => x.CreatedAt)
           .Take(count)
           .ToListAsync();
}
