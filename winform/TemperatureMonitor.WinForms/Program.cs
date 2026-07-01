using TemperatureMonitor.WinForms.Forms;

namespace TemperatureMonitor.WinForms;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new FormDashboard());
    }
}
