using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using RazorDoIt.Data;

namespace RazorDoIt.Web
{
    // メモ: IIS6 または IIS7 のクラシック モードの詳細については、
    // http://go.microsoft.com/?LinkId=9394801 を参照してください

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute{View="~/Views/Shared/Error.cshtml"});
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            //routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Api",
                "Api/{action}",
                new { controller = "Api", Action = "Index" }
            );

            routes.MapRoute(
                "Feed",
                "Feed/{format}",
                new { controller = "Home", Action = "Feed" , format = FeedFormat.Atom}
            );

            routes.MapRoute(
                "Permalink",
                "now/{key}",
                new { controller = "Home", Action = "Index" }
            );

            routes.MapRoute(
                "Default", // ルート名
                "{controller}/{action}/{id}", // パラメーター付きの URL
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // パラメーターの既定値
            );

        }

        protected void Application_Start()
        {
            SiteRepository.Bootstrap();
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
        }
    }
}