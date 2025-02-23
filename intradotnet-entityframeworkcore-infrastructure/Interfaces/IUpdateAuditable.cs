namespace IntraDotNet.EntityFrameworkCore.Infrastructure.Interfaces;

public interface IUpdateAuditable
{
    public DateTimeOffset? LastUpdateOn { get; set; }
    public string? LastUpdateBy { get; set; }
}