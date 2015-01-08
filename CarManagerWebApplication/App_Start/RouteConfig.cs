using System.Web.Mvc;
using System.Web.Routing;

namespace CarManagerWebApplication
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");


            routes.MapRoute(
                name: "Home",
                url: "Home/{action}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Contact",
                url: "Home/Contact",
                defaults: new { controller = "Home", action = "Contact", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "AcountRoute",
                url: "Account/{action}/{id}",
                defaults: new { controller = "Account", action = "Login", id = UrlParameter.Optional }
                );

            routes.MapRoute(
                name: "CompanyRoute",
                url: "{companyName}/{controller}/{action}/{id}",
                defaults: new {controller = "Home", action = "Index", id = UrlParameter.Optional}
                );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );


        }
    }
}