namespace DoctorsLog.Entities;

public class RecipeItem : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    public long RecipeId { get; set; }
    public Recipe Recipe { get; set; } = default!;
}
