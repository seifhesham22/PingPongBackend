namespace PingPong.API.Features.Shared
{
    public class Result<T>
    {
        public T? Value { get; }
        public Error? Error { get; }
        public bool IsSuccess { get; }
        private Result(T value)
        {
            Value = value;
            Error = null;
            IsSuccess = true;
        }
        private Result(Error error)
        {
            Value = default;
            Error = error;
            IsSuccess = false;
        }
        public static Result<T> Success(T value) => new Result<T>(value);
        public static implicit operator Result<T>(Error error) => new Result<T>(error);

        public IResult Match(Func<T, IResult> OnSuccess, Func<Error, IResult> OnFailure)
        {
            return IsSuccess ? OnSuccess(Value!) : OnFailure(Error!);
        }
    }

    /// <summary>
    /// A result for operations that carry no value on success — only success or failure.
    /// </summary>
    public class Result
    {
        public Error? Error { get; }
        public bool IsSuccess { get; }

        private Result()
        {
            Error = null;
            IsSuccess = true;
        }
        private Result(Error error)
        {
            Error = error;
            IsSuccess = false;
        }
        public static Result Success() => new Result();
        public static Result Failure(Error error) => new Result(error);

        public IResult Match(Func<IResult> OnSuccess, Func<Error, IResult> OnFailure)
        {
            return IsSuccess ? OnSuccess() : OnFailure(Error!);
        }
    }
}