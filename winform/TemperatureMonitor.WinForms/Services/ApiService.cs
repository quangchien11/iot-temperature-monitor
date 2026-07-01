using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using TemperatureMonitor.WinForms.Models;

namespace TemperatureMonitor.WinForms.Services;

/// <summary>
/// Lớp gọi REST API của ASP.NET Core bằng HttpClient.
/// Đổi BaseUrl cho khớp địa chỉ server đang chạy.
/// </summary>
public class ApiService
{
    // Đổi cổng/địa chỉ nếu API chạy nơi khác.
    public static string BaseUrl { get; set; } = "http://localhost:5094";

    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

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
