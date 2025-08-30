namespace DoctorsLog.Services.GoogleServices;

using DoctorsLog.Entities;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.IO;

public class GoogleSheetsService : IGoogleSheetsService
{
    private readonly SheetsService service;
    private readonly string spreadsheetId;
    private readonly string sheetName;
    private readonly string dateFormat;
    private readonly TimeZoneInfo timeZone;

    public GoogleSheetsService(IConfiguration config)
    {
        sheetName = config["ApplicationName"]!;
        dateFormat = config["DateFormat"]!;
        timeZone = TimeZoneInfo.FindSystemTimeZoneById(config["TimeZone"]!);

        spreadsheetId = config["GoogleSheets:SpreadsheetId"]
                         ?? throw new InvalidOperationException("SpreadsheetId not found in config");

        var credsSection = config.GetSection("GoogleSheets:Credentials");
        var credsJson = JObject.FromObject(credsSection.GetChildren().ToDictionary(c => c.Key, c => c.Value));

        GoogleCredential credential;
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(credsJson.ToString()));
        credential = GoogleCredential.FromStream(stream)
            .CreateScoped(SheetsService.Scope.Spreadsheets);

        service = new SheetsService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = sheetName
        });
    }

    public async Task<List<Subscription>> GetAllSubscriptionsAsync()
    {
        var range = $"{sheetName}!A:K";
        var request = service.Spreadsheets.Values.Get(spreadsheetId, range);
        var response = await request.ExecuteAsync();
        var values = response.Values;
        var list = new List<Subscription>();
        if (values == null || values.Count == 0) return list;

        foreach (var row in values.Skip(1))
        {
            var s = new Subscription
            {
                DeviceId = row.ElementAtOrDefault(0)?.ToString() ?? "",
                OwnerFullName = row.ElementAtOrDefault(1)?.ToString() ?? "",
                OwnerEmail = row.ElementAtOrDefault(2)?.ToString() ?? "",
                MachineName = row.ElementAtOrDefault(3)?.ToString() ?? "",
                Model = row.ElementAtOrDefault(4)?.ToString() ?? "",
                Manufacturer = row.ElementAtOrDefault(5)?.ToString() ?? "",
                StartDate = ParseFlexibleDate(row.ElementAtOrDefault(6)?.ToString()),
                EndDate = ParseFlexibleDate(row.ElementAtOrDefault(7)?.ToString()),
                IsActive = bool.TryParse(row.ElementAtOrDefault(8)?.ToString(), out var act) && act,
                LastSync = ParseFlexibleDate(row.ElementAtOrDefault(9)?.ToString()),
                CreatedAt = ParseFlexibleDate(row.ElementAtOrDefault(10)?.ToString())
            };
            list.Add(s);
        }

        return list;
    }

    public async Task<Subscription?> GetSubscriptionAsync(Subscription subscription)
    {
        var all = await GetAllSubscriptionsAsync();
        var sub = all.FirstOrDefault(x => x.DeviceId == subscription.DeviceId);

        if (sub is not null)
        {
            sub.StartDate = CheckDate(sub.StartDate, subscription.StartDate);
            sub.EndDate = CheckDate(sub.EndDate, subscription.EndDate);
            sub.CreatedAt = CheckDate(sub.CreatedAt, subscription.CreatedAt);
            sub.LastSync = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);
            await UpdateSubscriptionAsync(sub);
        }
        return sub;
    }

    public async Task UploadSubscriptionAsync(Subscription subscription)
    {
        subscription.LastSync = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);

        var range = $"{sheetName}!A:K";
        var values = new List<object?>
        {
            subscription.DeviceId,
            subscription.OwnerFullName,
            subscription.OwnerEmail,
            subscription.MachineName,
            subscription.Model,
            subscription.Manufacturer,
            subscription.StartDate.ToString(dateFormat),
            subscription.EndDate.ToString(dateFormat),
            subscription.IsActive,
            subscription.LastSync?.ToString(dateFormat),
            subscription.CreatedAt.ToString(dateFormat)
        };
        var request = service.Spreadsheets.Values.Append(
            new ValueRange { Values = [values] },
            spreadsheetId, range);
        request.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
        await request.ExecuteAsync();
    }

    public async Task UpdateSubscriptionAsync(Subscription subscription)
    {
        var all = await GetAllSubscriptionsAsync();
        var index = all.FindIndex(s => s.DeviceId == subscription.DeviceId);
        if (index == -1) return;

        subscription.LastSync = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);

        var range = $"{sheetName}!A{index + 2}:K{index + 2}";
        var values = new List<object?>
        {
            subscription.DeviceId,
            subscription.OwnerFullName,
            subscription.OwnerEmail,
            subscription.MachineName,
            subscription.Model,
            subscription.Manufacturer,
            subscription.StartDate.ToString(dateFormat),
            subscription.EndDate.ToString(dateFormat),
            subscription.IsActive,
            subscription.LastSync?.ToString(dateFormat),
            subscription.CreatedAt.ToString(dateFormat)
        };
        var body = new ValueRange { Values = [values] };
        var request = service.Spreadsheets.Values.Update(body, spreadsheetId, range);
        request.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
        await request.ExecuteAsync();
    }

    #region Private Helpers

    private DateTime ParseFlexibleDate(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return DateTime.MinValue;

        var dateFormats = new[]
        {
            "yyyy-MM-dd HH:mm:ss",
            "yyyy-MM-dd",
            "yyyy/M/d HH:mm",
            "yyyy/M/d",
            "M/d/yyyy HH:mm",
            "M/d/yyyy",
            "d/M/yyyy HH:mm",
            "d/M/yyyy",
            "dd.MM.yyyy HH:mm",
            "dd.MM.yyyy",
            "yyyy-MM-ddTHH:mm:ss",
            "yyyy-MM-ddTHH:mm:ssZ",
            "yyyy-MM-ddTHH:mm:sszzz",
            "MMM d, yyyy",
            "MMMM d yyyy",
            "d MMM yyyy"
        };

        foreach (var format in dateFormats)
            if (DateTime.TryParseExact(input, format, CultureInfo.InvariantCulture,
                                       DateTimeStyles.None, out var parsed))
                return parsed;

        if (DateTime.TryParse(input, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
            return result;

        return DateTime.MinValue;
    }
    private static DateTime CheckDate(DateTime cloudDate, DateTime localDate)
    {
        if (cloudDate != DateTime.MinValue && cloudDate.TimeOfDay == TimeSpan.Zero)
        {
            if (localDate.TimeOfDay != TimeSpan.Zero)
                return cloudDate.Date.Add(localDate.TimeOfDay);

            return cloudDate.Date.Add(new TimeSpan(23, 59, 59));
        }

        return cloudDate;
    }

    #endregion
}
