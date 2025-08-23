
using DoctorsLog.Services;
using Microsoft.EntityFrameworkCore;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Controls;

namespace DoctorsLog;

public partial class MainWindow : Window
{
    private Grid patientsView;
    private Grid prescriptionsView;
    private IAppDbContext db;

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

    private void CollapseExpandButton_Click(object sender, RoutedEventArgs e)
    {
        if (SideNavColumn.Width.Value == 50)
        {
            SideNavColumn.Width = new GridLength(200);
            PatientsText.Visibility = Visibility.Visible;
            PrescriptionText.Visibility = Visibility.Visible;
            SettingsText.Visibility = Visibility.Visible;
            var scaleTransform = new ScaleTransform(-1, 1);
            ArrowIcon.RenderTransform = scaleTransform;
        }
        else
        {
            SideNavColumn.Width = new GridLength(50);
            PatientsText.Visibility = Visibility.Collapsed;
            PrescriptionText.Visibility = Visibility.Collapsed;
            SettingsText.Visibility = Visibility.Collapsed;
            var scaleTransform = new ScaleTransform(1, 1);
            ArrowIcon.RenderTransform = scaleTransform;
        }
    }

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
        // Retseptlar view'i yaratilmagan bo'lsa, uni yaratamiz
        if (prescriptionsView == null)
        {
            prescriptionsView = CreatePrescriptionsView();
        }
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
        var addButton = new Button { Content = "Qo'shish", Background = new SolidColorBrush(Color.FromRgb(76, 175, 80)), Foreground = Brushes.White, Padding = new Thickness(8, 4,4,4) }; // Padding xatosi to'g'rilandi
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
        editButton.SetValue(Button.PaddingProperty, new Thickness(8, 4,4,4));

        var deleteButton = new FrameworkElementFactory(typeof(Button));
        deleteButton.SetValue(Button.ContentProperty, "O'chirish");
        deleteButton.SetValue(Button.BackgroundProperty, new SolidColorBrush(Color.FromRgb(244, 67, 54)));
        deleteButton.SetValue(Button.ForegroundProperty, Brushes.White);
        deleteButton.SetValue(Button.BorderThicknessProperty, new Thickness(0));
        deleteButton.SetValue(Button.PaddingProperty, new Thickness(8, 4,4,4));

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
}