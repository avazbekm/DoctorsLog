namespace DoctorsLog.Windows;

using DoctorsLog.Entities;
using DoctorsLog.Services.Persistence;
using DoctorsLog.Services.Subscriptions;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

public partial class ActivationWindow : Window
{
    private readonly SubscriptionService ss;
    private readonly Subscription sb;
    private List<Subscription> subscriptions = [];
    public readonly IAppDbContext db;

    public ActivationWindow(IAppDbContext db, SubscriptionService ss, Subscription sb)
    {
        InitializeComponent();
        this.db = db;
        this.sb = sb;
        this.ss = ss;
        DeviceIdText.Text = sb.DeviceId;

        Loaded += ActivationWindow_Loaded;
    }

    private async void ActivationWindow_Loaded(object sender, RoutedEventArgs e)
    {
        subscriptions = await db.Subscriptions.ToListAsync();
    }

    private async void Activate_Click(object sender, RoutedEventArgs e)
    {
        string token = TokenBox.Text;

        if (!subscriptions.Any(s => s.ActivationToken == token)
                && LicenseValidator.TryValidateToken(token, sb.DeviceId, out DateTime endDate))
        {
            var subscription = new Subscription()
            {
                DeviceId = sb.DeviceId,
                EndDate = endDate,
                StartDate = DateTime.UtcNow,
                ActivationToken = token,
                MachineName = sb.MachineName,
                Manufacturer = sb.Manufacturer,
                Model = sb.Model,
                OwnerFullName = sb.OwnerFullName,
                OwnerEmail = sb.OwnerEmail,
                IsActive = true,
                CreatedAt = sb.CreatedAt,
            };

            db.Subscriptions.Add(subscription);
            await db.SaveAsync();

            StatusText.Text = "✅ Aktivatsiya muvaffaqiyatli!";
            StatusText.Foreground = Brushes.Green;
            StatusText.Visibility = Visibility.Visible;

            ActivateButton.IsEnabled = false;

            await Task.Delay(2000);
            DialogResult = true;
            Close();
        }
        else
        {
            StatusText.Text = "❌ Token noto‘g‘ri!";
            StatusText.Foreground = Brushes.Red;
            StatusText.Visibility = Visibility.Visible;
        }
    }

    private void CopyDeviceId_Click(object sender, RoutedEventArgs e)
    {
        Clipboard.SetText(sb.DeviceId);
        StatusText.Text = "📋 Device ID nusxalandi!";
        StatusText.Foreground = Brushes.DarkGreen;
        StatusText.Visibility = Visibility.Visible;
    }

    private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
    {
        var url = App.Config!["AppSettings:TelegramBotUrl"]; ;
        Process.Start(new ProcessStartInfo(url!) { UseShellExecute = true });
        e.Handled = true;
    }

    private void PhoneTextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        Clipboard.SetText(App.Config!["AppSettings:SupportPhone"]);
        StatusText.Text = "📋 Telefon raqami nusxalandi!";
        StatusText.Foreground = Brushes.Green;
        StatusText.Visibility = Visibility.Visible;
    }
}
