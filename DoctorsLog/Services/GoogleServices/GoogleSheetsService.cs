namespace DoctorsLog.Services.GoogleServices;

using DoctorsLog.Entities;

public class GoogleSheetsService(string spreadsheetId, string apiKey) : IGoogleSheetsService
{
    public Subscription? GetSubscription(string deviceId)
    {
        throw new NotImplementedException();
    }

    public void UploadSubscription(Subscription subscription)
    {
        throw new NotImplementedException();
    }
}