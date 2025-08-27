namespace DoctorsLog.Entities;

public class Subscription : BaseEntity
{
    public string DeviceId { get; set; } = string.Empty;
    public string MachineName { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;

    public string OwnerFullName { get; set; } = string.Empty;
    public string OwnerEmail { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }

    public DateTime? LastSync { get; set; }
}
