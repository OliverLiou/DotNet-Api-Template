namespace DotNetWebApiMssql.Settings;

public sealed class LdapSettings
{
    public const string SectionName = "LdapSettings";

    public string Server { get; set; } = string.Empty;

    public string ServiceUsername { get; set; } = string.Empty;

    public string ServicePassword { get; set; } = string.Empty;
}
