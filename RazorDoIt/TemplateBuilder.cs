using System;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Compilation;
using System.Web.WebPages;
using RazorDoIt;
using RazorDoIt.Models;

namespace RazorDoIt
{
    public class TemplateBuilder
    {
        public BuildResult Execute(string virtualPath)
        {
            BuildResult response = null;
            var basePath = TemplateVirtualPathProvider.BasePath;
            var types = BuildManager.GetCompiledType(virtualPath);
            var webPage = Activator.CreateInstance(types) as WebPageBase;
            response = new BuildResult(ExecutCore(basePath, virtualPath, webPage));

            return response;
        }

        public string ExecutCore(string basePath, string virtualPath, WebPageBase page)
        {
            var sb = new StringBuilder();
            using (var tw = new StringWriter(sb))
            {
                var context = new HttpContextWrapper(CreateContext(basePath, virtualPath, tw));
                var pageContext = new WebPageContext(context, page, null);
                page.PushContext(pageContext, tw);
                page.Execute();
                page.PopContext();
            }

            return sb.ToString();
        }

        private HttpContext CreateContext(string basePath, string virtualPath, TextWriter tw)
        {
            var url = "http://localhost/Templates/sample.csdoit";
            var request = new HttpRequest(Path.Combine(basePath, @"Templates\sample.cshtml"), url, null);
            var response = new HttpResponse(tw);
            var httpContext = new HttpContext(request, response);
            httpContext.ApplicationInstance = new HttpApplication();
            return httpContext;
        }
    }
}
