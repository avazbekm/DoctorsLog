namespace DoctorsLog.Pages;

using DoctorsLog.Entities;
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
            .Where(r => r.FollowUpDate != null && r.FollowUpDate >= DateTime.Today)
            .Include(nameof(Patient))
            .OrderBy(r => r.FollowUpDate)
            .Select(r => new
            {
                Name = $"{r.Patient!.FirstName} {r.Patient.LastName}",
                Phone = r.Patient.PhoneNumber,
                UpdatedAt = r.UpdatedAt.ToString(),
                Status = "Planned"
            })
            .Take(10)
            .ToListAsync();

        appointments =
        [
            new { Name = "Muqimjon Mamadaliyev", Phone = "5611666646", UpdatedAt = "2025-01-05", Status = "Planned" },
            new { Name = "Muqimjon Mamadaliyev", Phone = "5611666646", UpdatedAt = "2025-01-05", Status = "Planned" },
            new { Name = "Muqimjon Mamadaliyev", Phone = "5611666646", UpdatedAt = "2025-01-05", Status = "Planned" },
            new { Name = "Muqimjon Mamadaliyev", Phone = "5611666646", UpdatedAt = "2025-01-05", Status = "Planned" },
            new { Name = "Muqimjon Mamadaliyev", Phone = "5611666646", UpdatedAt = "2025-01-05", Status = "Planned" },
            new { Name = "Muqimjon Mamadaliyev", Phone = "5611666646", UpdatedAt = "2025-01-05", Status = "Planned" },
            new { Name = "Muqimjon Mamadaliyev", Phone = "5611666646", UpdatedAt = "2025-01-05", Status = "Planned" },
            new { Name = "Muqimjon Mamadaliyev", Phone = "5611666646", UpdatedAt = "2025-01-05", Status = "Planned" },
            new { Name = "Muqimjon Mamadaliyev", Phone = "5611666646", UpdatedAt = "2025-01-05", Status = "Planned" },
        ];

        lvUpcomingAppointments.ItemsSource = appointments;
    }
}

