namespace SimpleAuthApi.Configuration;

public class JwtSettings
{
    public required string Key { get; set; }
    public required long AccessTokenExpiresInMinutes { get; set; }
    public required long RefreshTokenExpiresInMinutes { get; set; }
}
