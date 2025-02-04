using System.ComponentModel.DataAnnotations;

namespace IntraDotNet.EntityFrameworkCore.Interfaces;

public interface IRowVersion
{
    [Timestamp]
    byte[]? RowVersion { get; set; }
}