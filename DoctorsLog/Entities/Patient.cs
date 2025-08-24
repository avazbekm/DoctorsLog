using System.ComponentModel.DataAnnotations.Schema;

namespace DoctorsLog.Entities;

public class Patient : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public List<Recipe> Recipes { get; set; } = default!;

    [NotMapped]
    public bool IsEditing { get; set; }
}
