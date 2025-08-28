namespace DoctorsLog.Services.Subscriptions;

using System.Security.Cryptography;
using System.Text;

public class LicenseGenerator
{
    public static string GenerateToken(string deviceId, int days, string privateKey)
    {
        string expireAt = DateTime.UtcNow.AddDays(days).ToString("O");
        string payload = $"{deviceId}/{expireAt}";
        var data = Encoding.UTF8.GetBytes(payload);

        using var rsa = RSA.Create();
        rsa.FromXmlString(privateKey);
        var signature = rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        return Convert.ToBase64String(Encoding.UTF8.GetBytes(payload)) + "." +
               Convert.ToBase64String(signature);
    }
}
