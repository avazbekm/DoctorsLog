namespace DoctorsLog.Services.Persistence;

using DoctorsLog.Entities;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Threading.Tasks;

class AppDbContext : DbContext, IAppDbContext
{
    public DbSet<Patient> Patients { get; set; }
    public DbSet<Recipe> Recipes { get; set; }
    public DbSet<RecipeTemplate> RecipeTemplates { get; set; }
    public DbSet<Subscription> SubscriptionsService { get; set; }
    public DbSet<Subscription> Subscriptions { get; set; }

    public async Task<int> SaveAsync(CancellationToken cancellationToken = default)
        => await SaveChangesAsync(cancellationToken);

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        string oneDrivePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "OneDrive",
            "ClinicApp",
            "clinic.db"
        );

        Directory.CreateDirectory(Path.GetDirectoryName(oneDrivePath)!);

        options.UseSqlite($"Data Source={oneDrivePath}");
    }
}
