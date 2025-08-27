using DoctorsLog.Entities;
using DoctorsLog.Pages;
using DoctorsLog.Services.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace DoctorsLog.Windows;

public partial class HistoryPatientWindow : Window
{
    private IAppDbContext db;
    private Patient patient;
    private List<Recipe> recipes;
    public HistoryPatientWindow(IAppDbContext db, Patient patient)
    {
        InitializeComponent();
        this.db = db;
        this.patient = patient;
        _ = InitializeRecipeFieldsAsync();
    }

    public async Task InitializeRecipeFieldsAsync()
    {
        recipes = await db.Recipes
            .Where(r => r.PatientId == patient.Id)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
        LoadPatientData();
    }

    public void LoadPatientData()
    {
        tbFirstName.Text = patient.FirstName.ToUpper();
        tbLastName.Text = patient.LastName.ToUpper();
        tbBithday.Text = $"{patient.DateOfBirth:dd.MM.yyyy}";
        tbAddress.Text = patient.Address.ToUpper();
        tbPhone.Text = patient.PhoneNumber;
        var recipesForGrid = recipes.Select(r => new RecipeGridModel
        {
            Id = r.Id,
            CreatedAt = r.CreatedAt,
            Type = r.Type,
            Content = r.Content,
            Title = !string.IsNullOrEmpty(r.Content) && r.Content.Length > 30
             ? r.Content.Substring(0, 30).Trim() + "..."
             : r.Content.Trim(),
        }).ToList();

        HistoryDataGrid.ItemsSource = recipesForGrid;
    }

    // Retsept nomini bosganda ishlaydigan hodisa
    private void RecipeNameButton_Click(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        var recipeId = (long)button.Tag;

        var recipe = recipes.Find(r => r.Id == recipeId);

        if (recipe != null)
        {
            string fullrecipe = $"Retsept nomi: {recipe.Type}\n" +
                               $"Sanasi: {recipe.CreatedAt:dd.MM.yyyy HH:mm}\n\n" +
                               $"{recipe.Content}";

            // Maxsus dialog oynasi yaratish
            Window dialog = new Window
            {
                Title = "Retsept to'liq matni",
                Width = 800,
                Height = 800,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = ResizeMode.CanResize
            };

            Grid grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // ScrollViewer bilan TextBlock
            ScrollViewer scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Margin = new Thickness(10)
            };
            TextBlock textBlock = new TextBlock
            {
                Text = fullrecipe,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(5),
                FontSize = 14
            };
            scrollViewer.Content = textBlock;
            Grid.SetRow(scrollViewer, 0);

            // Tugmalar paneli
            StackPanel buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(10)
            };

            // Pechat tugmasi
            Button printButton = new Button
            {
                Content = "Pechat",
                Margin = new Thickness(0, 0, 10, 0),
                Padding = new Thickness(10, 5, 10, 5)
            };
            printButton.Click += (s, args) =>
            {
                PrintDialog printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    FlowDocument document = new FlowDocument();
                    Paragraph paragraph = new Paragraph(new Run(fullrecipe));
                    document.Blocks.Add(paragraph);

                    IDocumentPaginatorSource paginator = document;
                    printDialog.PrintDocument(paginator.DocumentPaginator, "Retsept");
                }
            };

            // OK tugmasi
            Button okButton = new Button
            {
                Content = "OK",
                Padding = new Thickness(10, 5, 10, 5)
            };
            okButton.Click += (s, args) => dialog.Close();

            buttonPanel.Children.Add(printButton);
            buttonPanel.Children.Add(okButton);
            Grid.SetRow(buttonPanel, 1);

            grid.Children.Add(scrollViewer);
            grid.Children.Add(buttonPanel);
            dialog.Content = grid;
            dialog.ShowDialog();
        }
    }

    // Yangi retsept qo'shish tugmasi bosilganda ishlaydigan hodisa
    private void BtnNewRetsep_Click(object sender, RoutedEventArgs e)
    {
        // DataGridni yashirish
        HistoryDataGridBorder.Visibility = Visibility.Collapsed;
        btnNewRetsep.Visibility = Visibility.Hidden;

        // Frame'ni ko'rsatish
        MainFrame.Visibility = Visibility.Visible;

        // NewRetsepPage'ni yaratish va Frame'ga yuklash
        NewRetsepPage newRetsepPage = new(db, patient);
        MainFrame.Content = newRetsepPage;
        MainFrame.Navigate(new NewRetsepPage(db, patient));
    }

    private void PrintButton_Click(object sender, RoutedEventArgs e)
    {
        // Pechat logikasi...
    }

    public class RecipeGridModel
    {
        public long Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Type { get; set; }
        public string Content { get; set; }
        public string Title { get; set; }
    }
}