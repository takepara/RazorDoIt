using System;
using System.Configuration;
using System.Net;
using System.Security.Permissions;
using System.Threading;
using System.Web;
using System.Web.Compilation;
using System.Web.Mvc;
using System.Web.WebPages;
using RazorDoIt.Models;

namespace RazorDoIt.Sandbox.Controllers
{
    public class TemplateResult : ActionResult
    {
        public string VirtualPath { get; set; }

        public override void ExecuteResult(ControllerContext context)
        {
            var types = BuildManager.GetCompiledType(VirtualPath);
            var webPage = Activator.CreateInstance(types) as WebPageBase;
            InThread(context, (ctx) =>
            {
                var pageContext = new WebPageContext(context.HttpContext, webPage, null);
                webPage.PushContext(pageContext, ctx.HttpContext.Response.Output);
                webPage.Execute();
                webPage.PopContext();
            });
        }

        private void InThread(ControllerContext context, Action<ControllerContext> functor)
        {
            var timeout = int.Parse(ConfigurationManager.AppSettings["SandboxTimeout"] ?? "5");
            var thread = new Thread(()=>
            {
                try
                {
                    var fileIo = new FileIOPermission(FileIOPermissionAccess.Read,AppDomain.CurrentDomain.BaseDirectory);
                    fileIo.AllLocalFiles = FileIOPermissionAccess.NoAccess;
                    fileIo.PermitOnly();

                    functor(context);
                }
                catch(Exception e)
                {
                    // 全部書きだす
                    context.HttpContext.Response.Write(e.Message);
                }
            });
            thread.Start();
            thread.Join(timeout*1000);
            if(thread.IsAlive)
            {
                thread.Abort();
                throw new HttpException((int)HttpStatusCode.InternalServerError, "処理が長すぎ(" + timeout + "秒で終わらせてね)");
            }
        }
    }

    public class WebPageController : Controller
    {
        public ActionResult Index()
        {
            return Redirect(ConfigurationManager.AppSettings["FrontUri"]);
        }

        public string Now()
        {
            return DateTime.Now.ToString();
        }

        [ValidateInput(false)]
        public ActionResult Execute(Template template)
        {
            var store = DependencyResolver.Current.GetService<ITemplateStore>();

            var path = string.Format("~/{0}/{1}.{2}", 
                TemplateVirtualPathProvider.RootPath,
                Guid.NewGuid().ToString("n"),
                template.PageName);
            var virtualPath = VirtualPathUtility.ToAbsolute(path);

            store.Save(virtualPath, template.Razor);

            return new TemplateResult { VirtualPath = path };
        }

    }
}
