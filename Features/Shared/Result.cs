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
        public static Result<T> Failure(Error error) => new Result<T>(error);

        public IResult Match(Func<T, IResult> OnSuccess, Func<Error, IResult> OnFailure)
        {
            return IsSuccess ? OnSuccess(Value!) : OnFailure(Error!);
        }
    }
}