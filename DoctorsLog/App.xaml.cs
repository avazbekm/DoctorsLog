namespace DoctorsLog;

using DoctorsLog.Services;
using DoctorsLog.Services.GoogleServices;
using DoctorsLog.Services.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Windows;

public partial class App : Application
{
    public IAppDbContext db { get; private set; }
    public GoogleSheetsService SheetsService { get; private set; }
    public SubscriptionService SubscriptionService { get; private set; }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        db = new AppDbContext();
        ((AppDbContext)db).Database.Migrate();

        SheetsService = new GoogleSheetsService("spreadsheetId", "apiKey");
        SubscriptionService = new SubscriptionService(db, SheetsService);
        await SubscriptionService.InitializeSubscriptionAsync();

        var mainWindow = new MainWindow(db);
        mainWindow.Show();
    }
}
