using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;

namespace NextPipe.ActionFilters
{
    public class InfrastructureValidationFilter : ActionFilterAttribute
    {
        public InfrastructureValidationFilter()
        {
            // Ask for 
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Do validation logic here
            await base.OnActionExecutionAsync(context, next);
        }
    }
}