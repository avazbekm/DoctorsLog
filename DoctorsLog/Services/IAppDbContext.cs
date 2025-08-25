namespace DoctorsLog.Services;

using DoctorsLog.Entities;
using Microsoft.EntityFrameworkCore;

public interface IAppDbContext
{
    DbSet<Patient> Patients { get; }
    DbSet<Recipe> Recipes { get; }
    DbSet<RecipeTemplate> RecipeItems { get; }

    Task<int> SaveAsync(CancellationToken cancellationToken = default);
}
