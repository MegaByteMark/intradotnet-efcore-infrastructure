namespace IntraDotNet.EntityFrameworkCore.Interfaces;

public interface ISoftDeleteAuditable
{
    public DateTimeOffset? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

}