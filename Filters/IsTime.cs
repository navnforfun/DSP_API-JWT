// MySecondAsyncActionFilterAttribute
using System.IdentityModel.Tokens.Jwt;
using App;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using App;
namespace DSP_API.Configurations.Filters
{

    public class IsTime : Attribute, IAsyncActionFilter
    {

        public IsTime()
        {
        }
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {


            var token = (context.HttpContext.Request.Headers[HeaderNames.Authorization]).ToString().Substring(7);
            try
            {
                if (!_let.CheckTokenIsValid(token))
                {
                    context.Result = new ContentResult()
                    {
                        StatusCode = StatusCodes.Status401Unauthorized,
                        Content = "Your token time is expired!"
                    };
                    return;
                }
            }
            catch
            {
                context.Result = new ContentResult()
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Content = "Your token time is expired or invalid!"
                };
                return;
            }
            await next();

        }

    }
}

