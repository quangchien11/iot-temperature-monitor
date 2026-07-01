using Microsoft.AspNetCore.Mvc;
using TemperatureApi.Dtos;
using TemperatureApi.Services;

namespace TemperatureApi.Controllers;

[ApiController]
[Route("api/temperature")]
public class TemperatureController : ControllerBase
{
    private readonly ITemperatureService _service;

    public TemperatureController(ITemperatureService service) => _service = service;

    /// <summary>
    /// Arduino gọi endpoint này mỗi 10 giây để gửi nhiệt độ + độ ẩm.
    /// Phản hồi chứa IsAlert để Arduino bật/tắt LED và Buzzer.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<TemperatureResponseDto>> Post([FromBody] TemperatureInputDto input)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _service.SaveAsync(input);
        return Ok(result);
    }

    /// <summary>Lấy bản ghi nhiệt độ mới nhất.</summary>
    [HttpGet("latest")]
    public async Task<ActionResult<TemperatureDto>> GetLatest()
    {
        var dto = await _service.GetLatestAsync();
        return dto == null ? NotFound("Chưa có dữ liệu.") : Ok(dto);
    }

    /// <summary>Tổng hợp số liệu cho Dashboard (nhiệt độ hiện tại, min/max ngày, trạng thái cảnh báo).</summary>
    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardDto>> GetDashboard() => Ok(await _service.GetDashboardAsync());

    /// <summary>Thống kê nhiệt độ theo giờ (24h gần nhất).</summary>
    [HttpGet("hourly")]
    public async Task<ActionResult<List<TemperatureStatisticDto>>> GetHourly() => Ok(await _service.GetHourlyAsync());

    /// <summary>Thống kê nhiệt độ theo ngày (7 ngày gần nhất).</summary>
    [HttpGet("daily")]
    public async Task<ActionResult<List<TemperatureStatisticDto>>> GetDaily() => Ok(await _service.GetDailyAsync());

    /// <summary>Thống kê nhiệt độ theo tuần (8 tuần gần nhất).</summary>
    [HttpGet("weekly")]
    public async Task<ActionResult<List<TemperatureStatisticDto>>> GetWeekly() => Ok(await _service.GetWeeklyAsync());

    /// <summary>Lịch sử các bản ghi gần nhất (mặc định 50).</summary>
    [HttpGet("history")]
    public async Task<ActionResult<List<TemperatureDto>>> GetHistory([FromQuery] int count = 50)
        => Ok(await _service.GetHistoryAsync(count));
}
