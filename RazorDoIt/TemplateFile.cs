using System.Text;
using System.Web.Mvc;

namespace RazorDoIt
{
    public class TemplateFile : ITemplateFile
    {
        public static string TemplateStoreKey = "templateStoreKey";
        private ITemplateStore _templateStore;

        public TemplateFile()
        {
            _templateStore = DependencyResolver.Current.GetService<ITemplateStore>();
        }
        public bool Exists(string path)
        {
            return _templateStore.Load(path) != null;
        }

        public string GetTemplate(string path)
        {
            var template = _templateStore.Load(path);
            if (template==null)
                return null;

            return template;
        }
    }
}
