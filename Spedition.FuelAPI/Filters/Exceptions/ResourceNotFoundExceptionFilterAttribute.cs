using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Spedition.Fuel.Shared.Exceptions;

namespace Spedition.FuelAPI.Filters.Exceptions
{
    /// <summary>
    /// Filters <see cref="ResourceNotFoundException"/> and provides <see cref="NotFoundResult"/>.
    /// </summary>
    public class ResourceNotFoundExceptionFilterAttribute : ExceptionFilterAttribute
    {
        /// <inheritdoc/>
        public override void OnException(ExceptionContext context)
        {
            if (context.Exception is ResourceNotFoundException)
            {
                context.Result = new NotFoundResult();
                context.ExceptionHandled = true;
            }
        }
    }
}
