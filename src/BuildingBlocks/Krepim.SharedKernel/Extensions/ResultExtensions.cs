using Krepim.SharedKernel.Results;
using Microsoft.AspNetCore.Http;

namespace Krepim.SharedKernel.Extensions
{
    public static class ResultExtensions
    {
        public static IResult ToProblemDetails(this Error error)
        {
            var statusCode = error.Type switch
            {
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status500InternalServerError
            };

            return Microsoft.AspNetCore.Http.Results.Problem(
                statusCode: statusCode,
                title: GetTitle(error.Type),
                type: "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                extensions: new Dictionary<string, object?> { { "errors", new[] { error } } }
            );
        }

        public static IResult Match<T>(
            this Result<T> result,
            Func<T, IResult> onSuccess)
        {
            return result.IsSuccess
                ? onSuccess(result.Value)
                : result.Error.ToProblemDetails();
        }

        public static IResult Match(
            this Result result,
            Func<IResult> onSuccess)
        {
            return result.IsSuccess
                ? onSuccess()
                : result.Error.ToProblemDetails();
        }

        private static string GetTitle(ErrorType type) => type switch
        {
            ErrorType.Validation => "Bad Request",
            ErrorType.NotFound => "Not Found",
            ErrorType.Conflict => "Conflict",
            _ => "Server Error"
        };
    }
}
