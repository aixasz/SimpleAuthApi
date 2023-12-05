namespace SimpleAuthApi.Domain;

public interface IUpdateTimeStamp
{
    DateTime Created { get; set; }
    DateTime? Updated { get; set; }
}