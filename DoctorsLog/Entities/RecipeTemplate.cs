namespace DoctorsLog.Entities;

public class RecipeTemplate : BaseEntity
{
    public string Type { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
