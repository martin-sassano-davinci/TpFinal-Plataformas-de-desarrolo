using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using tp4.Controllers;
using tp4.Models;

namespace tp4.Filters
{
    public class VerifySession : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            //var user = context.HttpContext.User;
            //var user = HttpContext.Session.GetString("usuario");
             //var user = HttpContext.Current.Session["usuario"];
             
           /* if (user == null)
            {
                if (context.Controller is AccessController == false)
                {
                    context.HttpContext.Response.Redirect("Access/Index");
                }
            }
           */
            if (!context.ModelState.IsValid)
            {
                if (context.Controller is AccessController == false)
                {
                    context.HttpContext.Response.Redirect("~/Access/Index");
                }
                context.Result = new BadRequestObjectResult(context.ModelState);
            } else
            {
                if (context.Controller is AccessController == true)
                {
                    context.HttpContext.Response.Redirect("~/Home/Index");
                }
            }
            base.OnActionExecuting(context);
        }
    }
}
