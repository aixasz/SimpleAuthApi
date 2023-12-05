using System.ComponentModel.DataAnnotations;

namespace SimpleAuthApi.Domain.Models;

public class UserCreateModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [DataType(DataType.Text)]
    public string FirstName { get; set; }

    [Required]
    [DataType(DataType.Text)]
    public string LastName { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; }
}