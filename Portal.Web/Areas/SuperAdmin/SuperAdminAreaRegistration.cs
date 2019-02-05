using System.Web.Mvc;

namespace Portal.Web.Areas.SuperAdmin
{
    public class SuperAdminAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "SuperAdmin";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                name: "SuperAdmin_default",
                url: "SuperAdmin/{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },                
                namespaces: new[] { "Portal.Web.Areas.SuperAdmin.Controllers" }
            );
        }
    }
}