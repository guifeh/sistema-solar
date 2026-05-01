namespace SolarSystem.Application.Common;

public class Result
{
    public bool IsSuccess { get; }
    public string Error { get; } = string.Empty;
    public bool IsFailure => !IsSuccess;

    protected Result(bool isSuccess, string error)
    {
        if (isSuccess && !string.IsNullOrEmpty(error))
            throw new InvalidOperationException("Success result cannot have an error.");
        if (!isSuccess && string.IsNullOrEmpty(error))
            throw new InvalidOperationException("Failure result must have an error.");

        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, string.Empty);
    public static Result Failure(string error) => new(false, error);
    public static Result<T> Success<T>(T value) => new(true, string.Empty, value);
    public static Result<T> Failure<T>(string error) => new(false, error, default!);
}

public class Result<T> : Result
{
    public T Value { get; }

    internal Result(bool isSuccess, string error, T value) : base(isSuccess, error)
    {
        Value = value;
    }
}
