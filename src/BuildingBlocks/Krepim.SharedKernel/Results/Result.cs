using Krepim.SharedKernel.Results.Interfaces;

namespace Krepim.SharedKernel.Results
{
    public readonly struct Result : IResult
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public Error Error { get; }

        private Result(bool isSuccess, Error error)
        {
            if (isSuccess && error != Error.None || !isSuccess && error == Error.None)
            {
                throw new ArgumentException("Invalid error state", nameof(error));
            }

            IsSuccess = isSuccess;
            Error = error;
        }

        public static Result Success() => new(true, Error.None);
        public static Result Failure(Error error) => new(false, error);
    }

    public readonly struct Result<T> : IResult
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public Error Error { get; }

        private readonly T? _value;

        private Result(bool isSuccess, Error error, T? value)
        {
            if (isSuccess && error != Error.None || !isSuccess && error == Error.None)
            {
                throw new ArgumentException("Invalid error state", nameof(error));
            }

            IsSuccess = isSuccess;
            Error = error;
            _value = value;
        }

        public T Value => IsSuccess // Доступ к Value разрешен только при успехе, иначе падаем.
            ? _value!
            : throw new InvalidOperationException($"The value of a failure result can't be accessed. Error: {Error.Code}");

        public static Result<T> Success(T value) => new(true, Error.None, value);
        public static Result<T> Failure(Error error) => new(false, error, default);

        public object Match(Func<object, Microsoft.AspNetCore.Http.IResult> value)
        {
            throw new NotImplementedException();
        }

        public static implicit operator Result<T>(T value) => Success(value);
    }
}
