namespace DoctorsLog;

using DoctorsLog.Services;
using Microsoft.EntityFrameworkCore;
using System.Windows;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        using var context = new AppDbContext();
        context.Database.Migrate();
    }
}
