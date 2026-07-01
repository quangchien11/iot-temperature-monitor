using TemperatureMonitor.Maui.Controls;
using TemperatureMonitor.Maui.Services;

namespace TemperatureMonitor.Maui.Views;

public partial class ChartPage : ContentPage
{
    private readonly ApiService _api;
    private readonly LineChartDrawable _drawable = new();
    private string _period = "hourly";

    public ChartPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
        ChartView.Drawable = _drawable;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = LoadAsync();
    }

    private async void OnPeriodClicked(object? sender, EventArgs e)
    {
        if (sender is Button b && b.CommandParameter is string p)
        {
            _period = p;
            HighlightButton(b);
            await LoadAsync();
        }
    }

    private void HighlightButton(Button active)
    {
        foreach (var b in new[] { BtnHourly, BtnDaily, BtnWeekly })
        {
            b.BackgroundColor = b == active ? Color.FromArgb("#1677FF") : Colors.Transparent;
            b.TextColor = b == active ? Colors.White : Color.FromArgb("#28344A");
        }
    }

    private async Task LoadAsync()
    {
        try
        {
            var data = await _api.GetStatisticsAsync(_period);
            _drawable.Data = data ?? new();
            ChartView.Invalidate(); // vẽ lại biểu đồ
        }
        catch
        {
            await DisplayAlert("Lỗi", "Không tải được dữ liệu biểu đồ.", "OK");
        }
    }
}
