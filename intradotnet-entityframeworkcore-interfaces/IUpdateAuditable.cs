using System.ComponentModel.DataAnnotations;

public interface IUpdateAuditable
{
    public DateTimeOffset? LastUpdateOn { get; set; }

    [MaxLength(50)]
    public string? LastUpdateBy { get; set; }
}