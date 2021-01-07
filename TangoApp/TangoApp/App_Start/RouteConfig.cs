using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace TangoApp
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Posts", action = "Index", id = UrlParameter.Optional }
            );
         

            routes.MapRoute(
                name: "Invite",
                url: "GroupMembers/Invite/{UserId}/{GrupId}",
                defaults: new
                {
                    controller = "GroupMembers",
                    action = "Invite",
                    UserId = UrlParameter.Optional,
                    GrupId = UrlParameter.Optional
                });
        }
    }
}
