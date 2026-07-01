using Microsoft.Extensions.Logging;
using TemperatureMonitor.Maui.Services;
using TemperatureMonitor.Maui.Views;

namespace TemperatureMonitor.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();
        // Lưu ý: nếu muốn dùng font tùy chỉnh, copy file .ttf vào Resources/Fonts
        // rồi gọi .ConfigureFonts(...). Ở đây dùng font hệ thống cho gọn.

        // Đăng ký dịch vụ + các trang (Dependency Injection)
        builder.Services.AddSingleton<ApiService>();
        builder.Services.AddTransient<DashboardPage>();
        builder.Services.AddTransient<HistoryPage>();
        builder.Services.AddTransient<ChartPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
