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


            var token = (context.HttpContext.Request.Headers[HeaderNames.Authorization]).ToString();
            if (!string.IsNullOrWhiteSpace(token))
            {
                context.Result = new ContentResult()
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Content = "Your token is not exits"
                };
                return;
            }
            if(token.Length < 10){
                 context.Result = new ContentResult()
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Content = "Your token is not exits"
                };
                return;
            }
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

