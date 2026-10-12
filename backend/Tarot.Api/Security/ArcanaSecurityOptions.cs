namespace Tarot.Api.Security;

public sealed class ArcanaSecurityOptions
{
    public const string CookieScheme = "Arcana.Session";
    public const string OidcScheme = "Keycloak";
    public string Authority { get; set; } = "http://localhost:8081/realms/arcana";
    public string PublicOrigin { get; set; } = "http://localhost:3000";
    public string ClientId { get; set; } = "arcana-web";
    public string ClientSecret { get; set; } = "";
    public bool AllowLoopbackHttp { get; set; }
    public string DataProtectionKeyPath { get; set; } = ".security-keys";
    public int IdleMinutes { get; set; } = 30;
    public int AbsoluteHours { get; set; } = 8;
    public int RevalidationSeconds { get; set; } = 60;

    public bool IsValid(bool development)
    {
        bool ValidUrl(string value) => Uri.TryCreate(value, UriKind.Absolute, out var uri)
            && string.IsNullOrEmpty(uri.UserInfo)
            && string.IsNullOrEmpty(uri.Query) && string.IsNullOrEmpty(uri.Fragment)
            && (uri.Scheme == "https" || (development && AllowLoopbackHttp
                && uri.Scheme == "http" && uri.IsLoopback));

        return ValidUrl(Authority) && ValidUrl(PublicOrigin)
            && new Uri(PublicOrigin).AbsolutePath == "/"
            && !string.IsNullOrWhiteSpace(ClientId) && ClientSecret.Length >= 32
            && !string.IsNullOrWhiteSpace(DataProtectionKeyPath)
            && IdleMinutes is >= 1 and <= 60 && AbsoluteHours is >= 1 and <= 24
            && RevalidationSeconds is >= 15 and <= 300;
    }

    public static string SafeReturnUrl(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 2000) return "/";
        var decoded = Uri.UnescapeDataString(value);
        return value.StartsWith('/') && decoded.StartsWith('/')
            && !decoded.StartsWith("//") && !decoded.Contains('\\')
            && !decoded.Any(char.IsControl) ? value : "/";
    }
}
