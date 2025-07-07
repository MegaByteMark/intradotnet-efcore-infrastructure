namespace IntraDotNet.EntityFrameworkCore.Infrastructure.Interfaces;

public interface IResult
{
    bool IsSuccess { get; }
    bool IsFailure { get; }
    public IEnumerable<string> Errors { get; }
    public string? AggregateErrors { get; }
}
