namespace DoctorsLog.Services.Subscriptions;

using System.Security.Cryptography;
using System.Text;

public class LicenseValidator
{
    private const string RSApublic = @"<RSAKeyValue><Modulus>y0DBIG5m4IsMaBaDmtvaP2fjAVphtywknErZ3sVyyjEuCBePQj23nQvUUE9FGM6JsoVTOXlDUnzZthXJ61n2K5mj7nneH6Gq5X/UsX8mDxu5WooAdIkduBFTirMyjHjn284jPHVomFVao9/cAJfgEMq1ZXQgrr8nuB1GaU1i/tpHTlH9QcPZiMmUGdeg83VaCI50PTU1MEMU9HoHru3cZ+o7XcsHFngJQ/xd+g4gT41CDPH1/+JpYKHpyzykdBGRQXXvx3aIG4x63astQlPFz6st7a+eu0C58GRxbl5lSjeqy0Z01mZTxNUuIT7RyeIBzRjbVn4CelNicfhk8JAJQQ==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

    public static bool TryValidateToken(string token, string deviceId, out DateTime endDate)
    {
        endDate = DateTime.MinValue;

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
