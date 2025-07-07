using IntraDotNet.EntityFrameworkCore.Infrastructure.Interfaces;

namespace IntraDotNet.EntityFrameworkCore.Infrastructure.Results;

public class Result: IResult
{
    public  bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public IEnumerable<string> Errors { get; } = [];

    private Result(bool isSuccess)
    {
        IsSuccess = isSuccess;
    }

    private Result(IEnumerable<string>? errors)
    {
        IsSuccess = false;
        Errors = errors ?? [];
    }

    public string? AggregateErrors
    {
        get
        {
            if (Errors is null || !Errors.Any())
            {
                return null;
            }

            return string.Join(Environment.NewLine, Errors);
        }
    }

    public static Result Success() => new(true);
    public static Result Failure(string error) => new([error]);
    public static Result Failure(IEnumerable<string> errors) => new(errors);
    public static Result Failure(Exception ex) => new(ex.Message is not null ? [ex.Message] : Array.Empty<string>());
    public static Result Failure(IEnumerable<Exception> exceptions) => new(exceptions.Select(e => e.Message).Where(m => m is not null));
    
    public static implicit operator Result(string error) => Failure(error);
}