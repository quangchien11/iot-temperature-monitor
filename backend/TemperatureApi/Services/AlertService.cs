using TemperatureApi.Dtos;
using TemperatureApi.Models;
using TemperatureApi.Repositories;

namespace TemperatureApi.Services;

public class AlertService : IAlertService
{
    private readonly IAlertRepository _alertRepo;

    public AlertService(IAlertRepository alertRepo) => _alertRepo = alertRepo;

    public async Task<AlertSettingDto> GetAsync()
    {
        var s = await _alertRepo.GetSettingAsync();
        return ToDto(s);
    }

    public async Task<AlertSettingDto> UpdateAsync(UpdateAlertSettingDto dto)
    {
        var s = await _alertRepo.UpdateSettingAsync(dto.MaxTemperature, dto.IsActive);
        return ToDto(s);
    }

    public async Task<List<AlertLogDto>> GetRecentLogsAsync(int count)
    {
        var logs = await _alertRepo.GetRecentLogsAsync(count);
        return logs.Select(x => new AlertLogDto
        {
            Id = x.Id,
            Temperature = x.Temperature,
            MaxTemperature = x.MaxTemperature,
            Message = x.Message,
            CreatedAt = x.CreatedAt
        }).ToList();
    }

    private static AlertSettingDto ToDto(AlertSetting s) => new()
    {
        Id = s.Id,
        MaxTemperature = s.MaxTemperature,
        IsActive = s.IsActive,
        UpdatedAt = s.UpdatedAt
    };
}
