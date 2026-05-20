using Krepim.SharedKernel.Extensions;
using Microsoft.AspNetCore.Http;

namespace Krepim.SharedKernel.Results.Filters
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
                return Microsoft.AspNetCore.Http.Results.Ok(genericResult.Value);
            }

            if (resultObj is Result result)
            {
                if (result.IsFailure)
                {
                    return result.Error.ToProblemDetails();
                }
                return Microsoft.AspNetCore.Http.Results.NoContent();
            }

            return resultObj;
        }
    }
}
