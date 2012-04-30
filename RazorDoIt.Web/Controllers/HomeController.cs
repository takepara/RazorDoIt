using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Net;
using System.ServiceModel.Syndication;
using System.Text;
using System.Web.Caching;
using System.Web.Mvc;
using System.Web.Security;
using RazorDoIt.Models;
using RazorDoIt.Services;

namespace RazorDoIt.Web.Controllers
{
    public class HomeController : AsyncController
    {
        private const string TemplateKey = "buildTemplate";
        private const string DefaultTempateKey = "defaultTemplate";
        private const string AuthReturnPathKey = "authReturnKey";

        private readonly SiteService _siteService = new SiteService();

        private string GetDefaultTemplate()
        {
            var path = Server.MapPath("~/Views/Shared/_DefaultTemplate.cshtml");
            var template = HttpContext.Cache[DefaultTempateKey] as string;
            if(string.IsNullOrEmpty(template))
            {
                template = System.IO.File.ReadAllText(path);
                HttpContext.Cache.Insert(DefaultTempateKey, template, new CacheDependency(path));
            }

            return template;
        }

        public ActionResult Feed(FeedFormat format)
        {
            var url = string.Format("{0}://{1}/now/", Request.Url.Scheme, Request.Url.Authority);
            var items = _siteService.FindRecentTemplates().ToFeed(url);
            var feed = new SyndicationFeed(items){Title = new TextSyndicationContent("Razor Do It")};
            return new FeedResult(feed, format);
        }

        public ActionResult Index(string key)
        {
            var template = _siteService.GetTemplate(key);

            if (!string.IsNullOrEmpty(key) && template.Id == 0)
                return RedirectToAction("Index", new { id = "" });

            var model = new Template(template);
            if (string.IsNullOrEmpty(model.Razor))
                model.Razor = GetDefaultTemplate();

            if (template.Id != 0)
            {
                ViewBag.Title = template.Title + " - Razor Do It";
            }
            else
            {
                var prev = TempData[TemplateKey] as Template;
                if (prev != null)
                    model = prev;
            }
            return View(model);
        }

        public ActionResult SignIn(string oauth_token, string oauth_verifier, string returnPath)
        {
            // OAuthのTokenがないならログインできないから！
            if (string.IsNullOrEmpty(oauth_token))
            {
                FormsAuthentication.SignOut();
                TempData[AuthReturnPathKey] = returnPath ?? Url.Action("Index", new { id = "" });
                return Redirect(_siteService.GetTwitterAuthUri(Request.Url.AbsoluteUri));
            }

            // ログインしてるのにユーザー情報が保持されてないならやり直させる。
            if (Request.IsAuthenticated)
            {
                if (_siteService.GetAccount(User.Identity.Name) == null)
                    return RedirectToAction("SignOut");
            }

            if (!string.IsNullOrEmpty(oauth_token) || !string.IsNullOrEmpty(oauth_verifier))
            {
                var account = _siteService.SignIn(oauth_token, oauth_verifier);
                if(account!=null)
                    FormsAuthentication.SetAuthCookie(account.UserName, true);
            }

            return SafetyRedirect((string)TempData[AuthReturnPathKey] ?? returnPath);
        }

        public ActionResult SignOut(string returnPath)
        {
            FormsAuthentication.SignOut();
            TempData.Clear();

            return SafetyRedirect(returnPath);
        }

        private ActionResult SafetyRedirect(string returnPath)
        {
            var redirectPath = returnPath;
            if (string.IsNullOrEmpty(returnPath) || !returnPath.StartsWith("/"))
                redirectPath = Url.Action("Index", new { id = "" });

            return Redirect(redirectPath);
        }

        [CommandName("Command","Save")]
        [HttpPost, Authorize, ValidateAntiForgeryToken]
        public ActionResult Save(Template template)
        {
            if (ModelState.IsValid)
            {
                var key = _siteService.SaveTemplate(User.Identity.Name, template);
                TempData.Remove(TemplateKey);
                return RedirectToRoute("Permalink", new { key });
            }

            TempData[TemplateKey] = template;

            return RedirectToAction("Index", new { id = "" });
        }

        [CommandName("Command","Copy")]
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Copy(Template template)
        {
            if (template != null && template.Id != 0)
            {
                template.ParentId = template.Id;
                template.Id = 0;
                TempData[TemplateKey] = template;
            }
            return RedirectToAction("Index", new { id = "" });
        }

        [CommandName("Command","Execute")]
        [HttpPost, ValidateAntiForgeryToken]
        public void ExecuteAsync(Template template)
        {
            AsyncManager.OutstandingOperations.Increment();
            var client = new WebClient { Encoding = Encoding.UTF8 };
            client.UploadValuesCompleted += (s, e) =>
            {
                if (e.Error == null)
                {
                    template.Response = Encoding.UTF8.GetString(e.Result);
                }
                else
                {
                    var exception = (WebException)e.Error;
                    var responseStream = exception.Response.GetResponseStream();
                    if (responseStream != null)
                        template.Response = new StreamReader(responseStream, Encoding.UTF8).ReadToEnd();
                }
                AsyncManager.Parameters["template"] = template;

                AsyncManager.OutstandingOperations.Decrement();
            };

            // SandboxにTemplateをPOSTしてビルド＆実行！
            // ※なのでTemplate内でIsPostを見ると常にtrue...
            var uri = new Uri(ConfigurationManager.AppSettings["SandboxUri"]);
            var values = new NameValueCollection();
            var extensions = new Dictionary<string, string>
                                 {
                                     {"cshtml", "csdoit"},
                                     {"vbhtml", "vbdoit"}
                                 };
            var pageName = "default." + extensions[template.Language ?? "cshtml"];
            values["Razor"] = template.Razor;
            values["PageName"] = pageName;
            client.UploadValuesAsync(uri, "POST", values);
        }

        public ActionResult ExecuteCompleted(Template template)
        {
            TempData[TemplateKey] = template;

            // ちょっとキモイね。
            // Ajaxでしか受け付けないんだろ？みたいなね。
            if (Request.IsAjaxRequest())
                return View("~/Views/Home/_Response.cshtml", template);

            return RedirectToAction("Index", new { id = "" });
        }
    }
}
