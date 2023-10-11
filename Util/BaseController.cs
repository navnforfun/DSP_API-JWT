using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Logging;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;

namespace DSP_API.Util
{
    [Route("api/[controller]/[action]")]
    public class BaseController : Controller
    {
        public string _Username
        {
            get
            {
                return HttpContext.Session.GetString("Username");

            }
            set
            {
                HttpContext.Session.SetString("Username", value);
            }
        }
        public string _Name
        {
            get
            {
                return HttpContext.Session.GetString("Name");

            }
            set
            {
                HttpContext.Session.SetString("Name", value);
            }
        }
        public string _UserRole
        {
            get
            {
                return HttpContext.Session.GetString("UserRole");

            }
            set
            {
                HttpContext.Session.SetString("UserRole", value);
            }
        }
        public int _UserId
        {
            get
            {
                return (int)HttpContext.Session.GetInt32("UserId");

            }
            set
            {
                HttpContext.Session.SetInt32("UserId", value);
            }
        }
        public bool _IsLogin
        {
            get
            {
                return !string.IsNullOrEmpty(_Username);
            }
        }
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            base.OnActionExecuted(context);

            // Log information about the action execution
            if (context.Exception != null)
            {
                // If there was an exception during the action execution, log the exception
                // System.Console.WriteLine(context.Exception.ToString(), "An exception occurred during the execution of the action method.");
            }
            else
            {
                // If the action executed successfully, log the action name and result
                // var actionName = context.ActionDescriptor.DisplayName;
                // var result = context.Result;
                // System.Console.WriteLine($"Action '{actionName}' executed successfully. Result: {result}");
            }
        }
    }
}
