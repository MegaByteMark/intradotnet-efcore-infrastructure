using System.ComponentModel.DataAnnotations;

namespace IntraDotNet.EntityFrameworkCore.Optimizations.Interfaces;

public interface IRowVersion
{
    [Timestamp]
    byte[] RowVersion { get; set; }
}