using System.Collections.Generic;
using System.Reflection;
using System.Web.Compilation;
using System.Web.Razor;
using System.Web.WebPages;
using System.Web.WebPages.Razor;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;

namespace RazorDoIt.Sandbox
{
    public static class PreApplicationStartCode
    {
        private static bool _started;
        public static void Start()
        {

            if (_started)
            {
                return;
            }
            _started = true;

            var field = typeof(RazorCodeLanguage).GetField("_services", BindingFlags.Static | BindingFlags.NonPublic);
            var services = field.GetValue(null) as IDictionary<string, RazorCodeLanguage>;
            if (services != null)
            {
                services.Add("csdoit", new CSharpRazorCodeLanguage());
                services.Add("vbdoit", new VBRazorCodeLanguage());
            }

            WebPageHttpHandler.RegisterExtension("csdoit");
            WebPageHttpHandler.RegisterExtension("vbdoit");
            BuildProvider.RegisterBuildProvider(".csdoit", typeof(RazorBuildProvider));
            BuildProvider.RegisterBuildProvider(".vbdoit", typeof(RazorBuildProvider));

            var assembly = Assembly.GetAssembly(typeof(System.Web.WebPages.WebPage));
            var moduleType = assembly.GetType("System.Web.WebPages.WebPageHttpModule");
            DynamicModuleUtility.RegisterModule(moduleType);
        }
    }
}
