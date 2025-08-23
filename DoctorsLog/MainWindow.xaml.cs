using System.Windows;
using DoctorsLog.Pages;
using DoctorsLog.Services;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using System.Windows.Media.Animation;

namespace DoctorsLog;

public partial class MainWindow : Window
{
    private Grid patientsView;
    private IAppDbContext db;

#nullable disable
    public MainWindow()
    {
        InitializeComponent();

        // XAML'dagi 'PatientsView' ni MainContentControl dan olib olamiz
        patientsView = (Grid)MainContentControl.Content;

        db = new AppDbContext();

        // 'MainContentControl' ichini boshqa elementlar bilan almashtirish uchun uni tozalaymiz
        MainContentControl.Content = null;

        // Ilova ishga tushganda faqat kartochkalar ko'rinishi uchun
        ShowInfoCards();
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

        SideNavPanel.BeginAnimation(FrameworkElement.WidthProperty, widthAnimation);
    }

    private void ToggleTextVisibility(double to)
    {
        var visibility = to == 200 ? Visibility.Visible : Visibility.Collapsed;

        PatientsText.Visibility = visibility;
        PrescriptionText.Visibility = visibility;
        SettingsText.Visibility = visibility;
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

    private void ShowInfoCards()
    {
        // Info kartalar panelini ko'rsatamiz va MainContentControl'ni tozalaymiz
        InfoCardsPanel.Visibility = Visibility.Visible;
        MainContentControl.Content = null;
    }

    private void ShowPatientsView()
    {
        // Info kartalar panelini yashiramiz
        InfoCardsPanel.Visibility = Visibility.Collapsed;

        // MainContentControl'ga bemorlar view'ini yuklaymiz
        MainContentControl.Content = patientsView;

        // Bemorlar ro'yxatini yangilash
        var patients = db.Patients.ToList();
        PatientsDataGrid.ItemsSource = patients;
        PatientsDataGrid.SelectedIndex = 0;
    }

    private void ShowPrescriptionsView()
    {
        // Info kartalar panelini yashiramiz
        InfoCardsPanel.Visibility = Visibility.Collapsed;

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
        var tb = sender as TextBox;
        string digits = Regex.Replace(tb.Text, @"[^\d]", "");
        if (digits.Length > 8)
            digits = digits.Substring(0, 8);
        string formatted = FormatDate(digits);
        tb.TextChanged -= DateTextBox_TextChanged;
        tb.Text = formatted;
        tb.CaretIndex = tb.Text.Length;
        tb.TextChanged += DateTextBox_TextChanged;
    }

    private string FormatDate(string input)
    {
        if (input.Length <= 2)
            return input;
        else if (input.Length <= 4)
            return input.Insert(2, ".");
        else if (input.Length <= 8)
            return input.Insert(2, ".").Insert(5, ".");
        else
            return input;
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
        {
            txtBirthDate.Text = selectedDate.ToString("dd.MM.yyyy");
        }
        popupBirthDate.IsOpen = false;
    }

    private void TbPhone_TextChanged(object sender, TextChangedEventArgs e) => FormatPhoneNumber((sender as TextBox)!);

    private void FormatPhoneNumber(TextBox textBox)
    {
        if (textBox == null) return;
        string text = textBox.Text?.Trim() ?? string.Empty;
        string digits = Regex.Replace(text, @"[^\d]", "");

        textBox.TextChanged -= TbPhone_TextChanged;

        try
        {
            if (digits.Length == 0 || !digits.StartsWith("998"))
            {
                digits = "998" + digits;
            }

            string formatted = "+998 ";
            if (digits.Length > 3)
            {
                formatted += digits.Substring(3, Math.Min(2, digits.Length - 3));
            }
            if (digits.Length > 5)
            {
                formatted += " " + digits.Substring(5, Math.Min(3, digits.Length - 5));
            }
            if (digits.Length > 8)
            {
                formatted += " " + digits.Substring(8, Math.Min(2, digits.Length - 8));
            }
            if (digits.Length > 10)
            {
                formatted += " " + digits.Substring(10, Math.Min(2, digits.Length - 10));
            }

            textBox.Text = formatted.TrimEnd();
            textBox.SelectionStart = textBox.Text.Length;
        }
        finally
        {
            textBox.TextChanged += TbPhone_TextChanged;
        }
    }

    private void TextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            if (sender == tbName)
                tbLastName.Focus();
            else if (sender == tbLastName)
                txtBirthDate.Focus();
            else if (sender == txtBirthDate)
                tbAddress.Focus();
            else if (sender == tbAddress)
                tbPhone.Focus();
            else if (sender == tbPhone)
                btnSave.Focus();
        }
    }

    private void PatientsView_Loaded(object sender, RoutedEventArgs e)
    {
        tbSearch.Focus();
    }
}