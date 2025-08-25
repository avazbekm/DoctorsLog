using DoctorsLog.Entities;
using DoctorsLog.Pages;
using DoctorsLog.Services;
using DoctorsLog.Windows;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace DoctorsLog;

public partial class MainWindow : Window
{
    private Grid patientsView;
    private RetseptPage prescriptionsPage;
    private IAppDbContext db;

#nullable disable
    public MainWindow()
    {
        InitializeComponent();

        // XAML'dagi 'PatientsView' ni MainContentControl dan olib olamiz
        patientsView = (Grid)MainContentControl.Content;

        db = new AppDbContext();

        MainContentControl.DataContext = db;
        MainContentControl.Content = new Dashboard(db);
    }

    #region Side Navigation Animation
    private void CollapseExpandButton_Click(object sender, RoutedEventArgs e)
    {
        double from = SideNavPanel.Width;
        double to = from == 50 ? 200 : 50;

        AnimateSideNavWidth(from, to);
        ToggleTextVisibility(to);
        AnimateArrowRotation(to);
    }

    private void AnimateSideNavWidth(double from, double to)
    {
        var widthAnimation = new DoubleAnimation
        {
            From = from,
            To = to,
            Duration = TimeSpan.FromMilliseconds(300),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
        };

        SideNavPanel.BeginAnimation(WidthProperty, widthAnimation);
    }

    private void ToggleTextVisibility(double to)
    {
        var visibility = to == 200 ? Visibility.Visible : Visibility.Collapsed;

        PatientsText.Visibility = visibility;
        PrescriptionText.Visibility = visibility;
        DashboardText.Visibility = visibility;
    }

    private void AnimateArrowRotation(double to)
    {
        var scale = to == 200 ? -1 : 1;

        var rotateAnimation = new DoubleAnimation
        {
            To = scale,
            Duration = TimeSpan.FromMilliseconds(300),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
        };

        if (ArrowIcon.RenderTransform is not ScaleTransform transform)
        {
            transform = new ScaleTransform(1, 1);
            ArrowIcon.RenderTransform = transform;
        }

        transform.BeginAnimation(ScaleTransform.ScaleXProperty, rotateAnimation);
    }
    #endregion

    private void PatientsButton_Click(object sender, RoutedEventArgs e)
    {
        ShowPatientsView();
    }

    private void PrescriptionButton_Click(object sender, RoutedEventArgs e)
    {
        ShowPrescriptionsView();
    }

    private async void ShowPatientsView()
    {
        // MainContentControl'ga bemorlar view'ini yuklaymiz
        MainContentControl.Content = patientsView;

        // Bemorlar ro'yxatini yangilash
        var patients = await db.Patients.OrderByDescending(p => p.CreatedAt).ToListAsync();
        PatientsDataGrid.ItemsSource = patients;
    }

    private void ShowPrescriptionsView()
    {

        // Create a Frame to host the RetseptPage
        var frame = new Frame();
        frame.Content = new RetseptPage();
        MainContentControl.Content = frame;
    }

  
    private void PatientsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
    }

    private void DateTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is not TextBox tb) return;

        tb.TextChanged -= DateTextBox_TextChanged;
        tb.Text = InputFormatter.FormatDate(tb.Text);
        tb.CaretIndex = tb.Text.Length;
        tb.TextChanged += DateTextBox_TextChanged;
    }

    private void DateTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
    {
        e.Handled = !Regex.IsMatch(e.Text, @"[\d\.]");
    }

    private void BtnBirthCalendar_Click(object sender, RoutedEventArgs e)
    {
        popupBirthDate.IsOpen = true;
    }

    private void CalendarBirthDate_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
    {
        if (calendarBirthDate.SelectedDate is DateTime selectedDate)
            txtBirthDate.Text = selectedDate.ToString("dd.MM.yyyy");

        popupBirthDate.IsOpen = false;
    }

    #region Phone TextBox
    private void TbPhone_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is not TextBox tb) return;

        tb.TextChanged -= TbPhone_TextChanged;

        var (formatted, caret) = InputFormatter.FormatPhoneInput(tb.Text);
        tb.Text = formatted;
        tb.CaretIndex = caret;

        tb.TextChanged += TbPhone_TextChanged;
    }

    private void TbPhone_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (sender is not TextBox tb) return;

        // Foydalanuvchi prefiksni o‘chirishga harakat qilsa, bloklaymiz
        if (tb.CaretIndex <= 5 &&
            (e.Key == Key.Back || e.Key == Key.Delete || e.Key == Key.Left))
        {
            e.Handled = true;
        }
    }
    #endregion

    private async void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        // 1️⃣ Input validation
        if (string.IsNullOrWhiteSpace(tbFirstName.Text) ||
            string.IsNullOrWhiteSpace(tbLastName.Text) ||
            string.IsNullOrWhiteSpace(tbAddress.Text) ||
            string.IsNullOrWhiteSpace(tbPhone.Text) ||
            string.IsNullOrWhiteSpace(txtBirthDate.Text))
        {
            MessageBox.Show("Iltimos, barcha maydonlarni to'ldiring.", "Eslatma", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!DateTime.TryParseExact(
                txtBirthDate.Text.Trim(),
                "dd.MM.yyyy",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out DateTime birthDate))
        {
            MessageBox.Show("Tug'ilgan sana noto'g'ri formatda. To'g'ri format: dd.MM.yyyy", "Xatolik", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // 2️⃣ Ma'lumotni saqlash
        var patient = new Patient
        {
            FirstName = tbFirstName.Text.Trim(),
            LastName = tbLastName.Text.Trim(),
            Address = tbAddress.Text.Trim(),
            PhoneNumber = tbPhone.Text.Trim(),
            DateOfBirth = birthDate,
            CreatedAt = DateTime.Now
        };

        db.Patients.Add(patient);
        bool isSaved = await db.SaveAsync() > 0;

        // 3️⃣ Natijani ko‘rsatish
        if (isSaved)
        {
            MessageBox.Show("Bemor muvaffaqiyatli qo'shildi!", "Muvaffaqiyat", MessageBoxButton.OK, MessageBoxImage.Information);
            ClearForm();
            tbFirstName.Focus();
            PatientsDataGrid.ItemsSource = await db.Patients.OrderByDescending(p => p.CreatedAt).ToListAsync();
        }
        else
        {
            MessageBox.Show("Bemorni qo'shishda xatolik yuz berdi.", "Xatolik", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    private void ClearForm()
    {
        tbFirstName.Clear();
        tbLastName.Clear();
        txtBirthDate.Clear();
        tbAddress.Clear();
        tbPhone.Clear();
    }

    private void TextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            switch (sender)
            {
                case var _ when sender == tbFirstName: tbLastName.Focus(); break;
                case var _ when sender == tbLastName: txtBirthDate.Focus(); break;
                case var _ when sender == txtBirthDate: tbAddress.Focus(); break;
                case var _ when sender == tbAddress: tbPhone.Focus(); break;
                case var _ when sender == tbPhone: btnSave.Focus(); break;
            }
    }



    private async void TbSearch_TextChanged(object sender, TextChangedEventArgs e)
    {
        string query = tbSearch.Text.Trim();
        var filtered = await SearchPatientsAsync(query);
        PatientsDataGrid.ItemsSource = filtered;
    }

    private async Task<List<Patient>> SearchPatientsAsync(string query)
    {
        var loweredQuery = query.ToLower();

        if (string.IsNullOrEmpty(loweredQuery))
            return await db.Patients.ToListAsync();

        return await db.Patients
            .Where(p =>
                p.FirstName.ToLower().Contains(loweredQuery) ||
                p.PhoneNumber.ToLower().Contains(loweredQuery) ||
                p.LastName.ToLower().Contains(loweredQuery) ||
                p.Address.ToLower().Contains(loweredQuery) ||
                p.DateOfBirth.ToString().Contains(loweredQuery)
            ).ToListAsync();
    }

    private void PatientsDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
    {
        e.Row.Header = (e.Row.GetIndex() + 1).ToString();
    }

    private void PatientsView_Loaded(object sender, RoutedEventArgs e)
    {
        tbSearch.Focus();
    }

    private async void BtnDelete_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is Patient patient)
        {
            var result = MessageBox.Show($"Bemorni o'chirishga ishonchingiz komilmi?\n{patient.FirstName} {patient.LastName}", "Tasdiqlash", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                db.Patients.Remove(patient);
                await db.SaveAsync();

                PatientsDataGrid.ItemsSource = await db.Patients.OrderByDescending(p => p.CreatedAt).ToListAsync();
            }
        }
    }

    private void BtnEditPatientInfo_Click(object sender, RoutedEventArgs e)
    {
        // 2. Faqat tanlangan qatorni tahrirlashga o‘tkazamiz
        if (sender is Button btn && btn.DataContext is Patient selectedPatient)
        {
            selectedPatient.IsEditing = true;
        }

        // 3. UI yangilansin
        PatientsDataGrid.Items.Refresh();
    }


    private void BtnConfirmEdit_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is Patient patient)
        {
            patient.IsEditing = false;
            // Bu yerda ma'lumotni saqlash logikasini qo‘shishing mumkin
        }
        PatientsDataGrid.Items.Refresh();
    }

    private void BtnCancelEdit_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is Patient patient)
        {
            patient.IsEditing = false;
            // Istasang eski qiymatga qaytarish logikasini qo‘shishing mumkin
        }
        PatientsDataGrid.Items.Refresh();
    }

    private void DashboardButton_Click(object sender, RoutedEventArgs e)
    {
        MainContentControl.Content = new Dashboard(db);
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        HistoryPatientWindow historyPatientWindow = new HistoryPatientWindow();
        historyPatientWindow.Show();
    }
}

public static class InputFormatter
{
        public static (string formattedText, int caretIndex) FormatPhoneInput(string rawInput)
        {
            var digits = ExtractDigits(rawInput);
            var formatted = FormatUzbekPhone(digits);
            var caret = CalculateCaretIndex(digits.Length);
            return (formatted, caret);
        }

        // 👇 Private helper methods
        private static string ExtractDigits(string input)
        {
            var digits = new string([.. input.Where(char.IsDigit)]);
            return digits.StartsWith("998") ? digits.Substring(3) : digits;
        }

    private static string FormatUzbekPhone(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return "+998 ";

        var digits = new string(input.Where(char.IsDigit).ToArray());

        if (digits.StartsWith("998"))
            digits = digits.Substring(3);

        if (digits.Length > 9)
            digits = digits.Substring(0, 9);

        var formatted = "+998";

        if (digits.Length >= 1)
            formatted += " " + digits.Substring(0, 1);

        if (digits.Length >= 2)
            formatted = "+998 " + digits.Substring(0, 2);

        if (digits.Length >= 3)
            formatted += " " + digits.Substring(2, 1);

        if (digits.Length >= 4)
            formatted = "+998 " + digits.Substring(0, 2) + " " + digits.Substring(2, 2);

        if (digits.Length >= 5)
            formatted = "+998 " + digits.Substring(0, 2) + " " + digits.Substring(2, 3);

        if (digits.Length >= 6)
            formatted += " " + digits.Substring(5, 1);

        if (digits.Length >= 7)
            formatted = "+998 " + digits.Substring(0, 2) + " " + digits.Substring(2, 3) + " " + digits.Substring(5, 2);

        if (digits.Length >= 8)
            formatted += " " + digits.Substring(7, 1);

        if (digits.Length == 9)
            formatted = "+998 " + digits.Substring(0, 2) + " " + digits.Substring(2, 3) + " " + digits.Substring(5, 2) + " " + digits.Substring(7, 2);

        return formatted;
    }

    private static int CalculateCaretIndex(int digitCount)
        => digitCount switch
        {
            <= 2 => 5 + digitCount,
            <= 5 => 5 + 2 + 1 + (digitCount - 2),
            <= 7 => 5 + 2 + 1 + 3 + 1 + (digitCount - 5),
            _ => 5 + 2 + 1 + 3 + 1 + 2 + 1 + (digitCount - 7)
        };


    public static string FormatDate(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return "";

        var digits = new string(input.Where(char.IsDigit).ToArray());

        if (digits.Length > 8)
            digits = digits.Substring(0, 8); // max DDMMYYYY

        string day = "", month = "", year = "";

        if (digits.Length >= 1)
            day = digits.Substring(0, Math.Min(2, digits.Length));

        if (digits.Length >= 3)
            month = digits.Substring(2, Math.Min(2, digits.Length - 2));

        if (digits.Length >= 5)
            year = digits.Substring(4, Math.Min(4, digits.Length - 4));

        // Range checks
        if (!int.TryParse(day, out int d) || d > 31) day = "";
        if (!int.TryParse(month, out int m) || m > 12) month = "";
        if (!int.TryParse(year, out int y) || y > DateTime.Now.Year) year = "";

        var parts = new List<string>();
        if (!string.IsNullOrEmpty(day)) parts.Add(day);
        if (!string.IsNullOrEmpty(month)) parts.Add(month);
        if (!string.IsNullOrEmpty(year)) parts.Add(year);

        return string.Join(".", parts);
    }
}
public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool invert = parameter?.ToString() == "False";
        bool flag = value is bool b && b;
        return (flag ^ invert) ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (value is Visibility v) && v == Visibility.Visible;
    }
}
