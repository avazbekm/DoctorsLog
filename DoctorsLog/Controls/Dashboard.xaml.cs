namespace DoctorsLog.Pages;

using DoctorsLog.Services;
using Microsoft.EntityFrameworkCore;
using System.Windows.Controls;

public partial class Dashboard : UserControl
{
    private readonly IAppDbContext db;

    public Dashboard(IAppDbContext db)
    {
        InitializeComponent();
        this.db = db;

        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        var today = DateTime.Today;

        int count = await db.Patients
            .CountAsync(p => p.CreatedAt >= today && p.CreatedAt < today.AddDays(1));

        tbCountPatientsForTheDay.Text = string.Concat(count.ToString(), " ta");

        count = await db.Recipes
            .CountAsync(p => p.CreatedAt >= today && p.CreatedAt < today.AddDays(1));
        tbCountRcipeForTheDay.Text = string.Concat(count.ToString(), " ta");
    }
}

