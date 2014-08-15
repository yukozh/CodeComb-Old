using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace CodeComb.Web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Contest",
                url: "Contest/{id}",
                defaults: new { controller = "Contest", action = "Show" },
                constraints: new { id = @"\d+" }
            );

            routes.MapRoute(
                name: "ContestStatus",
                url: "Contest/Status/{id}",
                defaults: new { controller = "Status", action = "Index" },
                constraints: new { id = @"\d+" }
            );

            routes.MapRoute(
                name: "Status",
                url: "Status/{id}",
                defaults: new { controller = "Status", action = "Show" },
                constraints: new { id = @"\d+" }
            );

            routes.MapRoute(
                name: "Index",
                url: "{controller}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                constraints: new { id = @"\d+" }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
               name: "Verifying",
               url: "{controller}/{action}/{id}/{token}",
               defaults: new { controller = "Verify", action = "Register", id = UrlParameter.Optional }
           );
        }
    }
}
