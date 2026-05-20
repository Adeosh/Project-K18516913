using Krepim.SharedKernel.Extensions;
using Microsoft.AspNetCore.Http;
using System.Reflection;

namespace Krepim.SharedKernel.Results.Filters
{
    public class ResultEndpointFilter : IEndpointFilter
    {
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            var resultObj = await next(context);

            if (resultObj is Interfaces.IResult result)
            {
                if (result.IsFailure)
                    return result.Error.ToProblemDetails(); 

                var type = resultObj.GetType();
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Result<>))
                {
                    var valueProperty = type.GetProperty("Value", BindingFlags.Public | BindingFlags.Instance);
                    var value = valueProperty?.GetValue(resultObj);
                
                    return Microsoft.AspNetCore.Http.Results.Ok(value);
                }

                return Microsoft.AspNetCore.Http.Results.NoContent();
            }

            return resultObj;
        }
    }
}
