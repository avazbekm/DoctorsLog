namespace DoctorsLog.Windows;

using DoctorsLog.Entities;
using DoctorsLog.Services.Persistence;
using DoctorsLog.Services.Subscriptions;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

public partial class ActivationWindow : Window
{
    private readonly Subscription sb;
    public readonly IAppDbContext db;
    public ActivationWindow(IAppDbContext db, Subscription sb)
    {
        InitializeComponent();
        this.db = db;
        this.sb = sb;
        DeviceIdText.Text = sb.DeviceId;
    }

    private async void Activate_Click(object sender, RoutedEventArgs e)
    {
        string token = TokenBox.Text;

        if (LicenseValidator.TryValidateToken(token, sb.DeviceId, out DateTime endDate))
        {
            sb.EndDate = endDate;
            sb.IsActive = true;
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
        Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
        e.Handled = true;
    }
}
