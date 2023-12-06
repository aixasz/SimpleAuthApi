namespace SimpleAuthApi.Domain.Models.UserManagement;

public class UserUpdateModel
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
}