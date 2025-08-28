namespace DoctorsLog;

using DoctorsLog.Services.GoogleServices;
using DoctorsLog.Services.Persistence;
using DoctorsLog.Services.Subscriptions;
using Microsoft.EntityFrameworkCore;
using System.Windows;

public partial class App : Application
{
    public IAppDbContext? Db { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        Db = new AppDbContext();
        ((AppDbContext)Db).Database.Migrate();

        var sh = new GoogleSheetsService("spreadsheetId", "apiKey");
        var ss = new SubscriptionService(Db, sh);

        var mainWindow = new MainWindow(Db, ss);
        mainWindow.Show();
    }
}
