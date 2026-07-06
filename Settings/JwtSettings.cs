namespace DotNetWebApiMssql.Settings;

public sealed class JwtSettings
{
    public const string SectionName = "JwtSettings";

    public string SecretKey { get; set; } = string.Empty;

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    public int ExpiryInHours { get; set; } = 2;

    public int RefreshTokenExpiryInHours { get; set; } = 24;
}


