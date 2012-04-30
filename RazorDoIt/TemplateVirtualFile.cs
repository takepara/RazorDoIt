using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Hosting;
using System.Web.Mvc;
using RazorDoIt;

namespace RazorDoIt
{
    public class TemplateVirtualFile : VirtualFile
    {
        private readonly string _virtualPath;
        private readonly ITemplateFile _templateFile;
        
        public TemplateVirtualFile(string virtualPath)
            : base(virtualPath)
        {
            _virtualPath = virtualPath;
            _templateFile = new TemplateFile();
        }

        public bool Exists
        {
            get
            {
                return _templateFile.Exists(_virtualPath);
            }
        }

        public override Stream Open()
        {
            if (!Exists)
                return new MemoryStream();

            var template = _templateFile.GetTemplate(_virtualPath);
            var bom = new byte[] {0xEF, 0xBB, 0xBF};
            var stream = new MemoryStream(bom.Concat(Encoding.UTF8.GetBytes(template)).ToArray());

            
            return stream;
        }
     }
}
