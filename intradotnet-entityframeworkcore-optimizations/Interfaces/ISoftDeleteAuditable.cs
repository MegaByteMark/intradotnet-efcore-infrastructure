using System.ComponentModel.DataAnnotations;

public interface ISoftDeleteAuditable
{
    public DateTimeOffset? DeletedOn { get; set; }

    [MaxLength(50)]
    public string? DeletedBy { get; set; }

}