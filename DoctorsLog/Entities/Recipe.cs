namespace DoctorsLog.Entities;

public class Recipe : BaseEntity
{
    public string Type { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;

    public long PatientId { get; set; }
    public Patient? Patient { get; set; }
}
