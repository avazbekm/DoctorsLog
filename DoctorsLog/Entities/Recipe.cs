namespace DoctorsLog.Entities;

class Recipe : BaseEntity
{
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public int HeartRate { get; set; }
    public double Weight { get; set; }


    public int PatientId { get; set; }
    public Patient? Patient { get; set; }
}
