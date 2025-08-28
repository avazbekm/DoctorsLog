namespace DoctorsLog.Pages;

using DoctorsLog.Entities;
using DoctorsLog.Services.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Windows.Controls;

public partial class Dashboard : Page
{
    private readonly IAppDbContext db;

    public Dashboard(IAppDbContext db)
    {
        InitializeComponent();
        this.db = db;

        _ = LoadDataAsync();
        _ = LoadUpcomingAppointmentsAsync();
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

    private async Task LoadUpcomingAppointmentsAsync()
    {
        var appointments = await db.Recipes
            .Include(nameof(Patient))
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new
            {
                Name = $"{r.Patient!.FirstName} {r.Patient.LastName}",
                Phone = r.Patient.PhoneNumber,
                CreatedAt = r.CreatedAt.ToString("dd.MM.yyyy HH:mm"),
            })
            .Take(10)
            .ToListAsync();

        lvUpcomingAppointments.ItemsSource = appointments;
    }
}

