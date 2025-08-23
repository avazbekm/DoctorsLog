using DoctorsLog.Entities;
using DoctorsLog.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace DoctorsLog;

public partial class MainWindow : Window
{
    private Grid patientsView;
    private Grid prescriptionsView;
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
        tbPatientsCount.Text = db.Patients.ToList().Count.ToString();
        tbRecipeCount.Text = db.Recipes.Count().ToString();

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
        // Retseptlar view'i yaratilmagan bo'lsa, uni yaratamiz
        prescriptionsView ??= CreatePrescriptionsView();
        // MainContentControl'ga retseptlar view'ini yuklaymiz
        MainContentControl.Content = prescriptionsView;
    }

    private Grid CreatePrescriptionsView()
    {
        var grid = new Grid();
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        // Birinchi qator: ComboBox va Qo'shish tugmasi
        var topPanel = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
        var comboBox = new ComboBox { Width = 200, Margin = new Thickness(0, 0, 10, 0) };
        comboBox.Items.Add("Retsept 1");
        comboBox.Items.Add("Retsept 2");
        var addButton = new Button { Content = "Qo'shish", Background = new SolidColorBrush(Color.FromRgb(76, 175, 80)), Foreground = Brushes.White, Padding = new Thickness(8, 4, 4, 4) }; // Padding xatosi to'g'rilandi
        topPanel.Children.Add(comboBox);
        topPanel.Children.Add(addButton);
        Grid.SetRow(topPanel, 0);
        grid.Children.Add(topPanel);

        // Ikkinchi qator: TextBox, + Button va DataGrid
        var bottomGrid = new Grid();
        bottomGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        bottomGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var drugPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 10) };
        var drugTextBox = new TextBox { Width = 250, Margin = new Thickness(0, 0, 5, 0) };
        var addDrugButton = new Button { Content = "+", Width = 30, Background = new SolidColorBrush(Color.FromRgb(76, 175, 80)), Foreground = Brushes.White };
        drugPanel.Children.Add(drugTextBox);
        drugPanel.Children.Add(addDrugButton);
        Grid.SetRow(drugPanel, 0);
        bottomGrid.Children.Add(drugPanel);

        var dataGrid = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            RowBackground = new SolidColorBrush(Color.FromRgb(249, 249, 249)),
            AlternatingRowBackground = new SolidColorBrush(Color.FromRgb(240, 240, 240))
        };

        var tartibColumn = new DataGridTextColumn { Header = "T/r", Width = new DataGridLength(50) };
        var doriColumn = new DataGridTextColumn { Header = "Dori nomi", Width = new DataGridLength(1, DataGridLengthUnitType.Star) };
        var amallarColumn = new DataGridTemplateColumn { Header = "Amallar", Width = DataGridLength.Auto };

        var amallarTemplate = new DataTemplate();
        var stackPanel = new FrameworkElementFactory(typeof(StackPanel));
        stackPanel.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);

        // Xatolik to'g'rilandi: Style'ni chaqirmasdan, tugma xususiyatlari to'g'ridan-to'g'i berildi.
        var editButton = new FrameworkElementFactory(typeof(Button));
        editButton.SetValue(Button.ContentProperty, "O'zgartirish");
        editButton.SetValue(Button.BackgroundProperty, new SolidColorBrush(Color.FromRgb(76, 175, 80)));
        editButton.SetValue(Button.ForegroundProperty, Brushes.White);
        editButton.SetValue(Button.MarginProperty, new Thickness(0, 0, 5, 0));
        editButton.SetValue(Button.BorderThicknessProperty, new Thickness(0));
        editButton.SetValue(Button.PaddingProperty, new Thickness(8, 4, 4, 4));

        var deleteButton = new FrameworkElementFactory(typeof(Button));
        deleteButton.SetValue(Button.ContentProperty, "O'chirish");
        deleteButton.SetValue(Button.BackgroundProperty, new SolidColorBrush(Color.FromRgb(244, 67, 54)));
        deleteButton.SetValue(Button.ForegroundProperty, Brushes.White);
        deleteButton.SetValue(Button.BorderThicknessProperty, new Thickness(0));
        deleteButton.SetValue(Button.PaddingProperty, new Thickness(8, 4, 4, 4));

        stackPanel.AppendChild(editButton);
        stackPanel.AppendChild(deleteButton);
        amallarTemplate.VisualTree = stackPanel;
        amallarColumn.CellTemplate = amallarTemplate;

        dataGrid.Columns.Add(tartibColumn);
        dataGrid.Columns.Add(doriColumn);
        dataGrid.Columns.Add(amallarColumn);

        Grid.SetRow(dataGrid, 1);
        bottomGrid.Children.Add(dataGrid);

        Grid.SetRow(bottomGrid, 1);
        grid.Children.Add(bottomGrid);

        return grid;
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
            txtBirthDate.Text = selectedDate.ToString("dd.MM.yyyy");

        popupBirthDate.IsOpen = false;
    }

    private void TbPhone_TextChanged(object sender, TextChangedEventArgs e) 
        => FormatPhoneNumber((sender as TextBox)!);

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
    private void TextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            if (sender == tbFirstName)
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

    private async void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        var patient = new Entities.Patient
        {
            FirstName = tbFirstName.Text.Trim(),
            LastName = tbLastName.Text.Trim(),
            Address = tbAddress.Text.Trim(),
            PhoneNumber = tbPhone.Text.Trim(),
            DateOfBirth = DateOnly.ParseExact(txtBirthDate.Text.Trim(), "dd.MM.yyyy"),
            CreatedAt = DateTimeOffset.Now
        };

        db.Patients.Add(patient);
        var isSaved = await db.SaveAsync() > 0;
        if (isSaved)
        {
            MessageBox.Show("Bemor muvaffaqiyatli qo'shildi!", "Muvaffaqiyat", MessageBoxButton.OK, MessageBoxImage.Information);
            tbFirstName.Clear();
            tbLastName.Clear();
            txtBirthDate.Clear();
            tbAddress.Clear();
            tbPhone.Clear();
            tbFirstName.Focus();
            PatientsDataGrid.ItemsSource = db.Patients.ToList();
        }
        else
        {
            MessageBox.Show("Bemorni qo'shishda xatolik yuz berdi.", "Xatolik", MessageBoxButton.OK, MessageBoxImage.Error);
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


    #region Index Column (Qator tartib raqamini aniqlash)
    private void IndexBlock_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is TextBlock tb)
        {
            var row = FindParent<DataGridRow>(tb);

            if (ItemsControl.ItemsControlFromItemContainer(row) is DataGrid dataGrid)
            {
                int index = dataGrid.ItemContainerGenerator.IndexFromContainer(row);
                tb.Text = (index + 1).ToString(); // 1-based index
            }
        }
    }


    private static T FindParent<T>(DependencyObject child) where T : DependencyObject
    {
        DependencyObject parent = VisualTreeHelper.GetParent(child);
        while (parent != null && parent is not T)
            parent = VisualTreeHelper.GetParent(parent);

        return parent as T;
    }
    #endregion

    private void PatientsView_Loaded(object sender, RoutedEventArgs e)
    {
        tbSearch.Focus();
    }
}