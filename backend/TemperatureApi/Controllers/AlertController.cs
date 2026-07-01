using Microsoft.AspNetCore.Mvc;
using TemperatureApi.Dtos;
using TemperatureApi.Services;

namespace TemperatureApi.Controllers;

[ApiController]
[Route("api/alert")]
public class AlertController : ControllerBase
{
    private readonly IAlertService _service;

    public AlertController(IAlertService service) => _service = service;

    /// <summary>Lấy cấu hình cảnh báo hiện hành.</summary>
    [HttpGet]
    public async Task<ActionResult<AlertSettingDto>> Get() => Ok(await _service.GetAsync());

    /// <summary>Cập nhật ngưỡng nhiệt độ và bật/tắt cảnh báo.</summary>
    [HttpPut]
    public async Task<ActionResult<AlertSettingDto>> Update([FromBody] UpdateAlertSettingDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return Ok(await _service.UpdateAsync(dto));
    }

    /// <summary>Nhật ký các lần cảnh báo gần nhất.</summary>
    [HttpGet("logs")]
    public async Task<ActionResult<List<AlertLogDto>>> GetLogs([FromQuery] int count = 20)
        => Ok(await _service.GetRecentLogsAsync(count));
}
