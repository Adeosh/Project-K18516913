using Krepim.Identity.Api.Extensions;
using Krepim.SharedKernel.Results;

namespace Krepim.Identity.Api.Infrastructure.Filters
{
    public class ResultEndpointFilter : IEndpointFilter
    {
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            var resultObj = await next(context);

            if (resultObj is Result<object> genericResult)
            {
                if (genericResult.IsFailure)
                {
                    return genericResult.Error.ToProblemDetails();
                }
                return Results.Ok(genericResult.Value);
            }

            if (resultObj is Result result)
            {
                if (result.IsFailure)
                {
                    return result.Error.ToProblemDetails();
                }
                return Results.NoContent();
            }

            return resultObj;
        }
    }
}
