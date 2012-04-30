using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using RazorDoIt;

namespace RazorDoIt
{
    public interface ITemplateStore
    {
        string Load(string key);
        void Save(string key, string buildTemplate);
    }

    public class HttpContextTemplateStore : ITemplateStore
    {
        public static string TemplateStoreKey = "templateStoreKey";
        private readonly HttpContextBase _context;

        public HttpContextTemplateStore(HttpContextBase context)
        {
            _context = context;
            var store = _context.Items[TemplateStoreKey] as IDictionary<string, string>;
            if (store == null)
                _context.Items[TemplateStoreKey] = new Dictionary<string, string>();
        }

        public string Load(string key)
        {
            var store = _context.Items[TemplateStoreKey] as IDictionary<string, string>;
            if (store == null)
                return null;

            return store[key];
        }

        public void Save(string key, string buildTemplate)
        {
            var store = _context.Items[TemplateStoreKey] as IDictionary<string, string>;
            if(store==null)
                return;

            store[key] = buildTemplate;
        }
    }
}
