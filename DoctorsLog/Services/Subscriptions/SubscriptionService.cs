namespace DoctorsLog.Services.Subscriptions;

using DoctorsLog.Entities;
using DoctorsLog.Services.GoogleServices;
using DoctorsLog.Services.Persistence;
using DoctorsLog.Windows;
using System.Management;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

public class SubscriptionService(IAppDbContext db, IGoogleSheetsService sheetsService)
{
    private const int TrialDays = 90;

    public async Task<Subscription> InitializeSubscriptionAsync()
    {
        string deviceId = GetDeviceUniqueId();
        var sb = GetLocalSubscription(deviceId);

        if (sb is null)
        {
            var (FullName, Email) = GetShowUserInfoDialog();
            var manufacturer = GetWmiValue("Win32_ComputerSystem", "manufacturer");
            var model = GetWmiValue("Win32_ComputerSystem", "Model");
            sb = new Subscription
            {
                DeviceId = deviceId,
                MachineName = Environment.MachineName,
                Manufacturer = manufacturer,
                Model = model,
                OwnerFullName = FullName,
                OwnerEmail = Email,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(TrialDays),
                IsActive = false,
                LastSync = null
            };

            db.Subscriptions.Add(sb);
            await db.SaveAsync();

            //if (GetIsInternetAvailable())
            //    sheetsService.UploadSubscription(sb);
        }

        await HandleSubscriptionExpirationAsync(sb);
        return sb;
    }

    #region Private Helpers

    private Subscription GetLocalSubscription(string deviceId)
        => db.Subscriptions.FirstOrDefault(s => s.DeviceId == deviceId)!;

    private static (string FullName, string Email) GetShowUserInfoDialog()
    {
        var dialog = new UserInfoDialog();
        if (dialog.ShowDialog() == true)
        {
            return (dialog.FullName, dialog.Email);
        }
        else
        {
            Application.Current.Shutdown();
            return (string.Empty, string.Empty);
        }
    }

    private async Task HandleSubscriptionExpirationAsync(Subscription subscription)
    {
        if (DateTime.Now < subscription.EndDate)
            return;

        var window = new ActivationWindow(db, subscription);
        window.ShowDialog();
        Application.Current.Shutdown();
        subscription.IsActive = false;
        await db.SaveAsync();
    }

    public static string GetDeviceUniqueId()
    {
        string diskId = GetWmiValue("Win32_DiskDrive", "SerialNumber");
        byte[] inputBytes = Encoding.UTF8.GetBytes(diskId);
        byte[] hashBytes = MD5.HashData(inputBytes);
        return Convert.ToHexString(hashBytes)[..8];
    }

    private static string GetWmiValue(string className, string propertyName)
    {
        try
        {
            using var searcher = new ManagementObjectSearcher($"SELECT {propertyName} FROM {className}");
            return searcher.Get()
                           .Cast<ManagementObject>()
                           .FirstOrDefault()?[propertyName]?.ToString() ?? "";
        }
        catch { return string.Empty; }
    }

    private static bool GetIsInternetAvailable()
    {
        try
        {
            using Ping ping = new();
            PingReply reply = ping.Send("8.8.8.8", 2000);
            return reply.Status == IPStatus.Success;
        }
        catch
        {
            return false;
        }
    }

    #endregion
}
