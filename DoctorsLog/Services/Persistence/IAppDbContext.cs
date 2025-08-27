namespace DoctorsLog.Services.Persistence;

using DoctorsLog.Entities;
using Microsoft.EntityFrameworkCore;

public interface IAppDbContext
{
    DbSet<Patient> Patients { get; }
    DbSet<Recipe> Recipes { get; }
    DbSet<RecipeTemplate> RecipeTemplates { get; }
    DbSet<Subscription> Subscriptions { get; }

    Task<int> SaveAsync(CancellationToken cancellationToken = default);
}
