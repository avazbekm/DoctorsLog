namespace DoctorsLog.Entities;

class Recipe : BaseEntity
{
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Pulse { get; set; }
    public double Weight { get; set; }
    public string Recomendations { get; set; } = string.Empty;
    public string BloodPressure { get; set; } = string.Empty;

    public int PatientId { get; set; }
    public Patient? Patient { get; set; }
}
