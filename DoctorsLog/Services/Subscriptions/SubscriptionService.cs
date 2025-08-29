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
    private Subscription sb = default!;

    public async Task<Subscription> InitializeSubscriptionAsync()
    {
        sb = GetLocalSubscription()!;

        if (sb is null)
        {
            if (GetIsInternetAvailable())
            {
                (sb ??= new()).DeviceId = GetDeviceUniqueId();
                var cloudSub = await sheetsService.GetSubscriptionAsync(sb.DeviceId);

                if (cloudSub is not null)
                {
                    sb = cloudSub;
                    db.Subscriptions.Add(sb);
                    await db.SaveAsync();
                }
                else
                    await GenerateInitialSubscription();
            }
            else
                await GenerateInitialSubscription();
        }

        await SynchronizeToCloud();
        await HandleSubscriptionExpirationAsync(sb);
        return sb;
    }

    private async Task GenerateInitialSubscription()
    {
        sb.DeviceId = GetDeviceUniqueId();

        var (fullName, email) = GetShowUserInfoDialog();
        sb.MachineName = Environment.MachineName;
        sb.Manufacturer = GetWmiValue("Win32_ComputerSystem", "Manufacturer");
        sb.Model = GetWmiValue("Win32_ComputerSystem", "Model");
        sb.OwnerFullName = fullName;
        sb.OwnerEmail = email;
        sb.CreatedAt = DateTime.Now;
        sb.StartDate = DateTime.Now;
        sb.EndDate = DateTime.Now.AddDays(TrialDays);
        sb.LastSync = DateTime.Now;

        db.Subscriptions.Add(sb);
        await db.SaveAsync();
    }

    private async Task SynchronizeToCloud()
    {
        if (!GetIsInternetAvailable())
            return;

        var cloudSub = await sheetsService.GetSubscriptionAsync(sb.DeviceId);

        if (cloudSub is null)
        {
            await sheetsService.UploadSubscriptionAsync(sb);
        }
        else
        {
            sb.OwnerFullName = cloudSub.OwnerFullName;
            sb.OwnerEmail = cloudSub.OwnerEmail;
            sb.StartDate = cloudSub.StartDate;
            sb.EndDate = cloudSub.EndDate;
            sb.IsActive = cloudSub.IsActive;
            sb.LastSync = DateTime.Now;
        }

        await db.SaveAsync();
    }

    private Subscription? GetLocalSubscription()
        => db.Subscriptions.OrderByDescending(s => s.CreatedAt).FirstOrDefault();

    private async Task HandleSubscriptionExpirationAsync(Subscription subscription)
    {
        if (DateTime.Now < subscription.EndDate)
            return;

        subscription.IsActive = false;
        await db.SaveAsync();

        var window = new ActivationWindow(db, this, sb);
        window.ShowDialog();
        Application.Current.Shutdown();
    }

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
}
