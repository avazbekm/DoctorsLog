namespace DoctorsLog.Services;

using DoctorsLog.Entities;

public interface IGoogleSheetsService
{
    void UploadSubscription(Subscription subscription);
    Subscription? GetSubscription(string deviceId);
}