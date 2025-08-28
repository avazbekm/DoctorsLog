namespace DoctorsLog.Services.GoogleServices;

using DoctorsLog.Entities;

public interface IGoogleSheetsService
{
    void UploadSubscription(Subscription subscription);
    Subscription? GetSubscription(string deviceId);
}