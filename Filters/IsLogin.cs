// MySecondAsyncActionFilterAttribute
using App;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;

namespace DSP_API.Configurations.Filters
{

    public class IsLogin : Attribute, IAsyncActionFilter
    {

        public IsLogin()
        {
        }
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var Username = context.HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(Username))
            {
                context.Result = new ContentResult()
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Content = "Require Login!"
                };
                return;

            }
            await next();

        }
    }
}

