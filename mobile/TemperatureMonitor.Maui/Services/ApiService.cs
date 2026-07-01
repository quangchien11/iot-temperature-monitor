using System.Net.Http.Json;
using System.Text.Json;
using TemperatureMonitor.Maui.Models;

namespace TemperatureMonitor.Maui.Services;

/// <summary>
/// Lớp gọi REST API ASP.NET Core.
/// LƯU Ý địa chỉ BaseUrl theo từng nền tảng:
///   - Máy ảo Android (emulator): dùng 10.0.2.2 để trỏ về localhost của máy host.
///   - Điện thoại thật: dùng IP LAN của máy chạy API (vd 192.168.1.10).
///   - Windows: dùng localhost.
/// </summary>
public class ApiService
{
#if ANDROID
    public const string BaseUrl = "http://10.0.2.2:5094";
#else
    public const string BaseUrl = "http://localhost:5094";
#endif

    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public ApiService()
    {
        _http = new HttpClient { BaseAddress = new Uri(BaseUrl), Timeout = TimeSpan.FromSeconds(10) };
    }

    public Task<DashboardDto?> GetDashboardAsync() =>
        _http.GetFromJsonAsync<DashboardDto>("/api/temperature/dashboard", JsonOpts);

    public Task<List<TemperatureStatisticDto>?> GetStatisticsAsync(string period) =>
        _http.GetFromJsonAsync<List<TemperatureStatisticDto>>($"/api/temperature/{period}", JsonOpts);

    public Task<List<TemperatureDto>?> GetHistoryAsync(int count = 50) =>
        _http.GetFromJsonAsync<List<TemperatureDto>>($"/api/temperature/history?count={count}", JsonOpts);

    public Task<AlertSettingDto?> GetAlertAsync() =>
        _http.GetFromJsonAsync<AlertSettingDto>("/api/alert", JsonOpts);

    public async Task<bool> UpdateAlertAsync(double maxTemperature, bool isActive)
    {
        var res = await _http.PutAsJsonAsync("/api/alert", new { maxTemperature, isActive });
        return res.IsSuccessStatusCode;
    }
}
