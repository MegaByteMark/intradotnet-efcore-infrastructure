using System.ComponentModel.DataAnnotations;

namespace IntraDotNet.EntityFrameworkCore.Infrastructure.Interfaces;

public interface IValidatable
{
    Task<ValidationResult?> ValidateAsync(CancellationToken cancellationToken = default);
    ValidationResult? Validate();
}
