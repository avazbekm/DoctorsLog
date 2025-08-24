using System.ComponentModel;

namespace DoctorsLog.Models;

public class PatientModel : INotifyPropertyChanged
{
    public long Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public string DateOfBirth { get; set; } = string.Empty;
    public bool IsEditing { get; set; }

    public PatientModel Clone()
    {
        return (PatientModel)this.MemberwiseClone();
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

