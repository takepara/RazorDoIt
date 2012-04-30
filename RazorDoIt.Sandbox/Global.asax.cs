using System;
using System.Collections.Specialized;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using RazorDoIt;

namespace RazorDoIt.Sandbox
{
    // メモ: IIS6 または IIS7 のクラシック モードの詳細については、
    // http://go.microsoft.com/?LinkId=9394801 を参照してください

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute(){View = "~/Views/Shared/Error.cshtml"});
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // ルート名
                "{controller}/{action}/{id}", // パラメーター付きの URL
                new { controller = "WebPage", action = "Index", id = UrlParameter.Optional } // パラメーターの既定値
            );

        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

            var basePath = Context.Server.MapPath("~/");
            TemplateVirtualPathProvider.Bootstrap(basePath);

            // 使ってみたかったんだよ。
            var serviceLocator = new DelegateServiceLocator();
            serviceLocator.Entry<ITemplateStore>(
                () => new HttpContextTemplateStore(new HttpContextWrapper(HttpContext.Current))
            );
            DependencyResolver.SetResolver(serviceLocator);
        }

    }
}