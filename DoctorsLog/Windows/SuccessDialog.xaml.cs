using System.Windows;
using System.Windows.Threading;

namespace DoctorsLog.Windows;

/// <summary>
/// Interaction logic for SuccessDialog.xaml
/// </summary>
public partial class SuccessDialog : Window
{
    public SuccessDialog()
    {
        InitializeComponent();

        Loaded += SuccessDialog_Loaded;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Owner = Application.Current.MainWindow;
    }

    private void SuccessDialog_Loaded(object sender, RoutedEventArgs e)
    {
        var timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };

        timer.Tick += (s, args) =>
        {
            timer.Stop();
            this.Close();
        };

        timer.Start();
    }
}