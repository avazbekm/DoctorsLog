namespace DoctorsLog.Windows;

using System.Text.RegularExpressions;
using System.Windows;

public partial class UserInfoDialog : Window
{
    public string FullName { get; private set; }
    public string Email { get; private set; }

    public UserInfoDialog()
    {
        InitializeComponent();
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        string fullName = FullNameTextBox.Text.Trim();
        string email = EmailTextBox.Text.Trim();

        // Majburiy tekshiruvlar
        if (string.IsNullOrWhiteSpace(fullName))
        {
            MessageBox.Show("Iltimos, F.I.O kiriting!", "Xatolik", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!IsValidEmail(email))
        {
            MessageBox.Show("Iltimos, to‘g‘ri email manzilini kiriting!", "Xatolik", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        FullName = fullName;
        Email = email;

        DialogResult = true;
        Close();
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown(); // Ma'lumot kiritilmasa dastur yopiladi
    }

    private bool IsValidEmail(string email)
    {
        return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
    }
}
