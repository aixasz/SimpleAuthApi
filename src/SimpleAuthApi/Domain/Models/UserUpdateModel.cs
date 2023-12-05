namespace SimpleAuthApi.Domain.Models;

public class UserUpdateModel
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
}