using TemperatureMonitor.Maui.Services;

namespace TemperatureMonitor.Maui.Views;

public partial class HistoryPage : ContentPage
{
    private readonly ApiService _api;

    public HistoryPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = LoadAsync();
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
            var data = await _api.GetHistoryAsync(100);
            ListView.ItemsSource = data;
        }
        catch
        {
            await DisplayAlert("Lỗi", "Không tải được lịch sử.", "OK");
        }
    }
}
