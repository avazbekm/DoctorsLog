namespace DoctorsLog.Windows;

using System.Windows;

public partial class UserInfoDialog : Window
{
    public UserInfoDialog()
    {
        InitializeComponent();
    }

    public string FullName { get; internal set; }
    public string Email { get; internal set; }
}
