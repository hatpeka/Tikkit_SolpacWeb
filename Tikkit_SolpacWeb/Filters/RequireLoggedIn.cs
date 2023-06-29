using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Tikkit_SolpacWeb.Data;

public class RequireLoginAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var user = context.HttpContext.Session.GetString("UserEmail");

        if (string.IsNullOrEmpty(user))
        {
            context.Result = new RedirectToActionResult("Login", "Users", null);
        }

        base.OnActionExecuting(context);
    }
}