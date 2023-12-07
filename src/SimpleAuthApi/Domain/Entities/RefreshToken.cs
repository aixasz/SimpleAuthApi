using System.ComponentModel.DataAnnotations;

namespace SimpleAuthApi.Domain.Entities;

public class RefreshToken
{
    [Key]
    public string Token { get; set; }
    public DateTime ExpiryDate { get; set; }
    public bool IsRevoked { get; set; }

    public Guid UserId { get; set; }
    public virtual User? User { get; set; }
}