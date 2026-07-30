namespace TripUpdater.Domain.Common;

public sealed record Error(string Code, string Message);

public class Result
{
    private readonly List<Error> _errors;

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public IReadOnlyList<Error> Errors => _errors.AsReadOnly();

    protected Result(bool isSuccess, List<Error> errors)
    {
        IsSuccess = isSuccess;
        _errors = errors;
    }

    public static Result Success() => new(true, []);

    public static Result Failure(params Error[] errors) => new(false, [.. errors]);

    public static Result Failure(string code, string message) => new(false, [new Error(code, message)]);
}

public sealed class Result<T> : Result
{
    private readonly T? _value;

    private Result(bool isSuccess, T? value, List<Error> errors)
        : base(isSuccess, errors)
    {
        _value = value;
    }

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access Value on a failed result.");

    public static Result<T> Success(T value) => new(true, value, []);

    public new static Result<T> Failure(params Error[] errors) => new(false, default, [.. errors]);

    public new static Result<T> Failure(string code, string message) => new(false, default, [new Error(code, message)]);
}
