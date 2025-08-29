namespace DoctorsLog.Services.GoogleServices;

using DoctorsLog.Entities;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Globalization;

public class GoogleSheetsService : IGoogleSheetsService
{
    private readonly SheetsService _service;
    private readonly string spreadsheetId;
    private readonly string sheetName = "DoctorsLog";
    private readonly TimeZoneInfo tashkentTz = TimeZoneInfo.FindSystemTimeZoneById("Asia/Tashkent");
    private readonly string dateFormat = "yyyy-MM-dd HH:mm";

    public GoogleSheetsService(IConfiguration config)
    {
        spreadsheetId = config["GoogleSheets:SpreadsheetId"]
                         ?? throw new InvalidOperationException("SpreadsheetId not found in config");

        var credsSection = config.GetSection("GoogleSheets:Credentials");
        var credsJson = JObject.FromObject(credsSection.GetChildren().ToDictionary(c => c.Key, c => c.Value));

        GoogleCredential credential;
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(credsJson.ToString()));
        credential = GoogleCredential.FromStream(stream)
            .CreateScoped(SheetsService.Scope.Spreadsheets);

        _service = new SheetsService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "DoctorsLog"
        });
    }

    public async Task<List<Subscription>> GetAllSubscriptionsAsync()
    {
        var range = $"{sheetName}!A:K";
        var request = _service.Spreadsheets.Values.Get(spreadsheetId, range);
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
                StartDate = DateTime.TryParseExact(row.ElementAtOrDefault(6)?.ToString(),
                                dateFormat, null, DateTimeStyles.None, out var sd) ? sd : DateTime.MinValue,
                EndDate = DateTime.TryParseExact(row.ElementAtOrDefault(7)?.ToString(),
                                dateFormat, null, DateTimeStyles.None, out var ed) ? ed : DateTime.MinValue,
                IsActive = bool.TryParse(row.ElementAtOrDefault(8)?.ToString(), out var act) && act,
                LastSync = DateTime.TryParseExact(row.ElementAtOrDefault(9)?.ToString(),
                                dateFormat, null, DateTimeStyles.None, out var ls) ? ls : DateTime.MinValue,
                CreatedAt = DateTime.TryParseExact(row.ElementAtOrDefault(10)?.ToString(),
                                dateFormat, null, DateTimeStyles.None, out var c) ? c : DateTime.MinValue
            };
            list.Add(s);
        }

        return list;
    }

    public async Task<Subscription?> GetSubscriptionAsync(string deviceId)
    {
        var all = await GetAllSubscriptionsAsync();
        var sub = all.FirstOrDefault(x => x.DeviceId == deviceId);
        if (sub is not null)
        {
            sub.LastSync = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tashkentTz);
            await UpdateSubscriptionAsync(sub);
        }
        return sub;
    }

    public async Task UploadSubscriptionAsync(Subscription subscription)
    {
        subscription.LastSync = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tashkentTz);

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
        var request = _service.Spreadsheets.Values.Append(
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

        subscription.LastSync = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tashkentTz);

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
        var request = _service.Spreadsheets.Values.Update(body, spreadsheetId, range);
        request.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
        await request.ExecuteAsync();
    }
}
