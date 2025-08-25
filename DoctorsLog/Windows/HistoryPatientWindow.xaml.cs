using DoctorsLog.Entities;
using DoctorsLog.Pages;
using DoctorsLog.Services;
using Microsoft.EntityFrameworkCore;
using System.Windows;
using System.Windows.Controls;

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
        tbFirstName.Text = patient.FirstName;
        tbLastName.Text = patient.LastName;
        tbBithday.Text = $"{patient.DateOfBirth:dd.MM.yyyy}";
        tbAddress.Text = patient.Address;
        tbPhone.Text = patient.PhoneNumber;

        HistoryDataGrid.ItemsSource = recipes;
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

            MessageBox.Show(fullrecipe, "Retsept to'liq matni", MessageBoxButton.OK, MessageBoxImage.Information);
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
}