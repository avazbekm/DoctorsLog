namespace DoctorsLog;

using DoctorsLog.Services.GoogleServices;
using DoctorsLog.Services.Persistence;
using DoctorsLog.Services.Subscriptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Windows;

public partial class App : Application
{
    public IAppDbContext? Db { get; private set; }
    public static IConfiguration? Config { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        Config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

        Db = new AppDbContext();
        ((AppDbContext)Db).Database.Migrate();

        var sh = new GoogleSheetsService(Config);
        var ss = new SubscriptionService(Db, sh);

        var mainWindow = new MainWindow(Db, ss);
        mainWindow.Show();
    }
}
