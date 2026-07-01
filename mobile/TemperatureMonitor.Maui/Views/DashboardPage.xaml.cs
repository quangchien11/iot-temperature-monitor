using TemperatureMonitor.Maui.Services;

namespace TemperatureMonitor.Maui.Views;

public partial class DashboardPage : ContentPage
{
    private readonly ApiService _api;
    private IDispatcherTimer? _timer;
    private bool _lastAlert;

    public DashboardPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = LoadAsync();

        // Tự động làm mới mỗi 5 giây
        _timer = Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(5);
        _timer.Tick += async (_, _) => await LoadAsync();
        _timer.Start();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _timer?.Stop();
    }

    private async void OnRefreshing(object? sender, EventArgs e)
    {
        await LoadAsync();
        Refresh.IsRefreshing = false;
    }

    private async Task LoadAsync()
    {
        try
        {
            var d = await _api.GetDashboardAsync();
            if (d == null) return;

            LblTemp.Text = $"{d.CurrentTemperature:0.0} °C";
            LblHumid.Text = $"{d.CurrentHumidity:0.0}%";
            LblMax.Text = $"{d.MaxTemperatureToday:0.0}°";
            LblMin.Text = $"{d.MinTemperatureToday:0.0}°";
            LblUpdated.Text = d.LastUpdated.HasValue ? $"Cập nhật: {d.LastUpdated:HH:mm:ss dd/MM}" : "";

            LblStatus.Text = d.IsAlert ? "⚠ CẢNH BÁO!" : "● BÌNH THƯỜNG";
            StatusBadge.BackgroundColor = d.IsAlert
                ? Color.FromArgb("#66FF4D4F")   // đỏ mờ khi cảnh báo
                : Color.FromArgb("#33FFFFFF");  // trắng mờ khi bình thường
            AlertBanner.IsVisible = d.IsAlert;
            AlertText.Text = $"CẢNH BÁO: {d.CurrentTemperature:0.0}°C vượt ngưỡng {d.MaxTemperatureThreshold}°C!";

            // Nếu chuyển từ bình thường -> cảnh báo thì hiện thông báo 1 lần.
            if (d.IsAlert && !_lastAlert)
                await DisplayAlert("⚠ CẢNH BÁO NHIỆT ĐỘ",
                    $"Nhiệt độ {d.CurrentTemperature:0.0}°C đã vượt ngưỡng {d.MaxTemperatureThreshold}°C!", "Đã hiểu");
            _lastAlert = d.IsAlert;

            // Nạp cấu hình cảnh báo (lần đầu)
            if (string.IsNullOrEmpty(EntryMax.Text))
            {
                var s = await _api.GetAlertAsync();
                if (s != null) { EntryMax.Text = s.MaxTemperature.ToString("0.#"); SwitchActive.IsToggled = s.IsActive; }
            }
        }
        catch
        {
            LblUpdated.Text = "Không kết nối được server.";
        }
    }

    private async void OnSaveAlert(object? sender, EventArgs e)
    {
        if (!double.TryParse(EntryMax.Text, out var max))
        {
            LblSaveMsg.TextColor = Colors.Red;
            LblSaveMsg.Text = "Giá trị ngưỡng không hợp lệ.";
            return;
        }
        bool ok = await _api.UpdateAlertAsync(max, SwitchActive.IsToggled);
        LblSaveMsg.TextColor = ok ? Colors.Green : Colors.Red;
        LblSaveMsg.Text = ok ? "✔ Đã lưu!" : "✖ Lưu thất bại.";
    }
}
