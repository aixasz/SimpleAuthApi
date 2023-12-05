using Microsoft.AspNetCore.Identity;

namespace SimpleAuthApi.Domain.Entities;

public class User : IdentityUser<Guid>, IUpdateTimeStamp, ISoftDelete
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public DateTime Created { get; set; }
    public DateTime? Updated { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? Deleted { get; set; }
}
