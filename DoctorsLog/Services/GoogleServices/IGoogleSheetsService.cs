namespace DoctorsLog.Services.GoogleServices;

using DoctorsLog.Entities;

public interface IGoogleSheetsService
{
    Task<List<Subscription>> GetAllSubscriptionsAsync();
    Task<Subscription?> GetSubscriptionAsync(Subscription subscription);
    Task UploadSubscriptionAsync(Subscription subscription);
    Task UpdateSubscriptionAsync(Subscription subscription);
}