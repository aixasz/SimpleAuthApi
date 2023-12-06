namespace SimpleAuthApi.Configuration;

public class Jwt
{
    public string AppSettingSection { get; } = "Jwt";
    public required string Key { get; set; }
    public required long ExpiresInMinutes { get; set; }
}
