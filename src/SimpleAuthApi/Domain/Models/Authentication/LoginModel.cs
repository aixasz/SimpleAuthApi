namespace SimpleAuthApi.Domain.Models.Authentication;

public class LoginModel
{
    public required string Username { get; set; }
    public required string Password { get; set; }
}
