namespace DoctorsLog;

using DoctorsLog.Entities;
using DoctorsLog.Pages;
using DoctorsLog.Services.Persistence;
using DoctorsLog.Windows;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

public partial class MainWindow : Window
{
    private Grid patientsView;
    private IAppDbContext db;
    private long? editingPatientId = null;

#nullable disable
    public MainWindow(IAppDbContext db)
    {
        InitializeComponent();
        patientsView = (Grid)MainContentControl.Content;

        this.db = db;

        MainContentControl.DataContext = db;
        MainContentControl.Content = new Frame
        {
            Content = new Dashboard(db)
        };
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

        // Har bir bemorning ism-sharifini katta harflarga o'tkazamiz
        var capitalizedPatients = patients.Select(p =>
        {

            if (p.FirstName != null)
            {
                p.FirstName = p.FirstName.ToUpper();
            }
            if (p.LastName != null)
            {
                p.LastName = p.LastName.ToUpper();
            }
            if (p.Address != null)
            {
                p.Address = p.Address.ToUpper();
            }
            return p;
        }).ToList();

        // Yangilangan ro'yxatni DataGrid'ga yuklaymiz
        PatientsDataGrid.ItemsSource = capitalizedPatients;
    }

    private void ShowPrescriptionsView()
    {
        Frame frame = new()
        {
            Content = new RetseptPage(db)
        };
        MainContentControl.Content = frame;
    }

    private void DateTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is not TextBox tb) return;

        tb.TextChanged -= DateTextBox_TextChanged;
        tb.Text = InputFormatter.FormatDate(tb.Text);
        tb.CaretIndex = tb.Text.Length;
        tb.TextChanged += DateTextBox_TextChanged;
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
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime birthDate))
        {
            MessageBox.Show("Tug'ilgan sana noto'g'ri formatda. To'g'ri format: dd.MM.yyyy", "Xatolik", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        bool isSaved = false;
        if (editingPatientId.HasValue)
        {
            var existingPatient = await db.Patients.FindAsync(editingPatientId.Value);
            if (existingPatient is not null)
            {
                existingPatient.FirstName = tbFirstName.Text.Trim();
                existingPatient.LastName = tbLastName.Text.Trim();
                existingPatient.Address = tbAddress.Text.Trim();
                existingPatient.PhoneNumber = tbPhone.Text.Trim();
                existingPatient.DateOfBirth = birthDate;

                isSaved = await db.SaveAsync() > 0;
            }
        }
        else
        {
            var patient = new Patient
            {
                FirstName = tbFirstName.Text.ToUpper().Trim(),
                LastName = tbLastName.Text.ToUpper().Trim(),
                Address = tbAddress.Text.ToUpper().Trim(),
                PhoneNumber = tbPhone.Text.Trim(),
                DateOfBirth = birthDate,
                CreatedAt = DateTime.Now
            };

            db.Patients.Add(patient);
            isSaved = await db.SaveAsync() > 0;
        }

        if (isSaved)
        {
            string message = editingPatientId.HasValue
                ? "Bemor ma'lumotlari yangilandi!"
                : "Bemor muvaffaqiyatli qo'shildi!";

            new SuccessDialog().ShowDialog();

            ClearForm();
            tbFirstName.Focus();
            editingPatientId = null;
            btnSave.Content = "Saqlash";

            PatientsDataGrid.ItemsSource = await db.Patients.OrderByDescending(p => p.CreatedAt).ToListAsync();
        }
        else
            MessageBox.Show("Ma'lumotni saqlashda xatolik yuz berdi.", "Xatolik", MessageBoxButton.OK, MessageBoxImage.Error);

        void ClearForm()
        {
            tbFirstName.Clear();
            tbLastName.Clear();
            txtBirthDate.Clear();
            tbAddress.Clear();
            tbPhone.Clear();
        }
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
        // Debounce qo'shamiz (500ms)
        var text = tbSearch.Text.Trim();

        // Oldingi taskni bekor qilish
        _searchCts?.Cancel();
        _searchCts = new CancellationTokenSource();

        try
        {
            await Task.Delay(500, _searchCts.Token);
            var filtered = await SearchPatientsAsync(text);
            PatientsDataGrid.ItemsSource = filtered;
        }
        catch (TaskCanceledException)
        {
            // Yangi qidiruv boshlandi, eski bekor qilindi
        }
    }

    private CancellationTokenSource _searchCts;

    private async Task<List<Patient>> SearchPatientsAsync(string query)
    {
        var allPatients = await db.Patients.AsNoTracking().ToListAsync();

        if (string.IsNullOrEmpty(query))
            return allPatients;

        var loweredQuery = query.ToLowerInvariant();
        var latinToCyrillic = ToCyrillic(loweredQuery);
        var cyrillicToLatin = ToLatin(loweredQuery);

        return allPatients.Where(p =>
            ContainsAny(p.FirstName, loweredQuery, latinToCyrillic, cyrillicToLatin) ||
            ContainsAny(p.LastName, loweredQuery, latinToCyrillic, cyrillicToLatin) ||
            ContainsAny(p.PhoneNumber, loweredQuery, latinToCyrillic, cyrillicToLatin) ||
            ContainsAny(p.Address, loweredQuery, latinToCyrillic, cyrillicToLatin)
        ).ToList();
    }

    private bool ContainsAny(string text, params string[] terms)
    {
        if (string.IsNullOrEmpty(text)) return false;

        var lowerText = text.ToLowerInvariant();
        return terms.Any(term => lowerText.Contains(term));
    }
    // Lotin → Kirill transliteratsiya (yaxshilangan)
    private string ToCyrillic(string input)
    {
        var result = input;

        // Birikmalar birinchi
        result = result
            .Replace("g'", "ғ").Replace("o'", "ў")
            .Replace("sh", "ш").Replace("ch", "ч")
            .Replace("yo", "ё").Replace("ya", "я").Replace("yu", "ю").Replace("ye", "е");

        // Yakka harflar
        result = result
            .Replace("a", "а").Replace("b", "б").Replace("d", "д")
            .Replace("e", "е").Replace("f", "ф").Replace("g", "г")
            .Replace("h", "ҳ").Replace("i", "и").Replace("j", "ж")
            .Replace("k", "к").Replace("l", "л").Replace("m", "м")
            .Replace("n", "н").Replace("o", "о").Replace("p", "п")
            .Replace("q", "қ").Replace("r", "р").Replace("s", "с")
            .Replace("t", "т").Replace("u", "у").Replace("v", "в")
            .Replace("x", "х").Replace("y", "й").Replace("z", "з");

        return result;
    }

    // Kirill → Lotin transliteratsiya (yaxshilangan)
    private string ToLatin(string input)
    {
        var result = input;

        // Birikmalar birinchi
        result = result
            .Replace("ғ", "g'").Replace("ў", "o'")
            .Replace("ш", "sh").Replace("ч", "ch")
            .Replace("ё", "yo").Replace("я", "ya").Replace("ю", "yu").Replace("е", "e");

        // Yakka harflar
        result = result
            .Replace("а", "a").Replace("б", "b").Replace("д", "d")
            .Replace("ф", "f").Replace("г", "g").Replace("ҳ", "h")
            .Replace("и", "i").Replace("ж", "j").Replace("к", "k")
            .Replace("л", "l").Replace("м", "m").Replace("н", "n")
            .Replace("о", "o").Replace("п", "p").Replace("қ", "q")
            .Replace("р", "r").Replace("с", "s").Replace("т", "t")
            .Replace("у", "u").Replace("в", "v").Replace("х", "x")
            .Replace("й", "y").Replace("з", "z");

        return result;
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
        if (sender is Button btn && btn.DataContext is Patient patient)
        {
            editingPatientId = patient.Id;
            tbFirstName.Text = patient.FirstName;
            tbLastName.Text = patient.LastName;
            txtBirthDate.Text = patient.DateOfBirth.ToString("dd.MM.yyyy");
            tbAddress.Text = patient.Address;
            tbPhone.Text = patient.PhoneNumber;
            tbFirstName.Focus();
            btnSave.Content = "Yangilash";
        }
    }

    private void DashboardButton_Click(object sender, RoutedEventArgs e)
    {
        MainContentControl.Content = new Frame
        {
            Content = new Dashboard(db)
        };
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        if (PatientsDataGrid.SelectedItem is Patient selectedPatient)
        {
            new HistoryPatientWindow(db, selectedPatient).ShowDialog();
        }
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
