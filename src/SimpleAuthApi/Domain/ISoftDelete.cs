namespace SimpleAuthApi.Domain;

public interface ISoftDelete
{
    bool IsDeleted { get; set; }

    DateTime? Deleted { get; set; }
}
