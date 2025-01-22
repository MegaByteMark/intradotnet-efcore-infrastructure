using System;
using System.ComponentModel.DataAnnotations;

public interface ICreateAuditable
{
    public DateTimeOffset? CreatedOn { get; set; }

    [MaxLength(50)]
    public string? CreatedBy { get; set; }
}