using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Hosting;

namespace RazorDoIt
{
    public class TemplateVirtualDirectory : VirtualDirectory
    {
        private IEnumerable _emptyList;
        public TemplateVirtualDirectory(string virtualPath)
            : base(virtualPath)
        {
            _emptyList = new ArrayList();
        }

        public override IEnumerable Directories
        {
            get { return _emptyList; }
        }

        public override IEnumerable Files
        {
            get { return _emptyList; }
        }

        public override IEnumerable Children
        {
            get { return _emptyList; }
        }
    }
}
