using System.ComponentModel.DataAnnotations;

namespace IntraDotNet.EntityFrameworkCore.Infrastructure.Interfaces;

public interface IRowVersion
{
    [Timestamp]
    byte[]? RowVersion { get; set; }
}