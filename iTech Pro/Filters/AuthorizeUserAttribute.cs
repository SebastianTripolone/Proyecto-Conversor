using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;

namespace iTech_Pro.Filters
{
    

    public class AuthorizeUserAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var usuario = context.HttpContext.Session.GetString("usuario");

            if (usuario == null)
            {
                // Si El usuario no ha iniciado sesión, redirige a la página de inicio de sesión
                context.Result = new RedirectResult("/");
            }

            base.OnActionExecuting(context);
        }
    }
}
