using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Spedition.FuelAPI.Filters.Exceptions;

namespace Spedition.FuelAPI.Filters
{
    /// <summary>
    /// Http response exception filter.
    /// </summary>
    public class HttpResponseExceptionFilterAttribute : ExceptionFilterAttribute
    {
        /// <inheritdoc/>
        public override void OnException(ExceptionContext context)
        {
            if (context.Exception is HttpResponseException exception)
            {
                context.Result = new ObjectResult(exception.Message ?? string.Empty)
                {
                    StatusCode = (int)exception.Status,
                };
                context.ExceptionHandled = true;
            }
        }
    }
}
