namespace IntraDotNet.EntityFrameworkCore.Interfaces;

public interface ICreateAuditable
{
    public DateTimeOffset? CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
}