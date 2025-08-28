namespace DoctorsLog.Services.Subscriptions;

using System.Security.Cryptography;
using System.Text;

public class LicenseValidator
{
    public static bool TryValidateToken(string token, string deviceId, out DateTime endDate)
    {
        endDate = DateTime.MinValue;
        string RSApublic = App.Config["PublicKey"]!;

        try
        {
            var parts = token.Split('.');
            if (parts.Length != 2) return false;

            var payloadBytes = Convert.FromBase64String(parts[0]);
            var signature = Convert.FromBase64String(parts[1]);
            var payload = Encoding.UTF8.GetString(payloadBytes);

            using var rsa = RSA.Create();
            rsa.FromXmlString(RSApublic);
            bool valid = rsa.VerifyData(payloadBytes, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            if (!valid) return false;

            var segments = payload.Split('/');
            if (segments.Length != 2) return false;

            if (segments[0] != deviceId) return false;

            if (!DateTime.TryParse(segments[1], null, System.Globalization.DateTimeStyles.RoundtripKind, out endDate))
                return false;

            if (DateTime.UtcNow > endDate)
                return false;

            return true;
        }
        catch
        {
            return false;
        }
    }
}
