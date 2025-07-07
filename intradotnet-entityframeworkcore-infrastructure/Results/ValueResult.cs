using IntraDotNet.EntityFrameworkCore.Infrastructure.Interfaces;

namespace IntraDotNet.EntityFrameworkCore.Infrastructure.Results;

public class ValueResult<T>: IResult
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T? Value { get; }
    public IEnumerable<string> Errors { get; }

    private ValueResult(T? value)
    {
        IsSuccess = true;
        Value = value;
        Errors = [];
    }

    private ValueResult(IEnumerable<string> errors)
    {
        IsSuccess = false;
        Value = default;
        Errors = errors;
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

    public static ValueResult<T> Success() => new(default(T));
    public static ValueResult<T> Success(T value) => new(value);
    public static ValueResult<T> Failure(string error) => new([error]);
    public static ValueResult<T> Failure(IEnumerable<string> errors) => new(errors);
    public static ValueResult<T> Failure(Exception ex) => new(ex.Message is not null ? [ex.Message] : Array.Empty<string>());
    public static ValueResult<T> Failure(IEnumerable<Exception> exceptions) => new(exceptions.Select(e => e.Message).Where(m => m is not null));


    public static implicit operator ValueResult<T>(T value) => Success(value);
    public static implicit operator ValueResult<T>(string error) => Failure(error);
}