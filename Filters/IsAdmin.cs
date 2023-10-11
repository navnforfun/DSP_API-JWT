// MySecondAsyncActionFilterAttribute
using App;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;

namespace DSP_API.Configurations.Filters
{

    public class IsAdmin : Attribute, IAsyncActionFilter
    {

        public IsAdmin()
        {
        }
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var UserRole = context.HttpContext.Session.GetString("UserRole");
            if (!UserRole.Contains("Admin"))
            {
                context.Result = new ContentResult()
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Content = "Require Admin!"
                };
                return;

            }
            await next();

        }
    }
}

