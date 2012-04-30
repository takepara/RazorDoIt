using System;
using System.Web;
using System.Web.Hosting;

namespace RazorDoIt
{
    public class TemplateVirtualPathProvider : VirtualPathProvider
    {
        public const string RootPath = "Templates";
        private static string _basePath;
        public static string BasePath
        {
            get { return _basePath; }
        }
        public TemplateVirtualPathProvider(string basePath)
        {
            _basePath = basePath;
        }

        public static void Bootstrap(string basePath)
        {
            HostingEnvironment.RegisterVirtualPathProvider(new TemplateVirtualPathProvider(basePath));
        }

        private bool IsDoItPath(string virtualPath)
        {
            var path = VirtualPathUtility.ToAppRelative(virtualPath);
            return path.StartsWith("~/" + RootPath + "/", StringComparison.InvariantCultureIgnoreCase);
        }

        public override bool FileExists(string virtualPath)
        {
            if(IsDoItPath(virtualPath))
            {
                var file = GetFile(virtualPath) as TemplateVirtualFile;
                return file.Exists;
            }

            return Previous.FileExists(virtualPath);
        }

        public override bool DirectoryExists(string virtualDir)
        {
            if (IsDoItPath(virtualDir))
            {
                return true;
            }

            return Previous.DirectoryExists(virtualDir);
        }

        public override VirtualFile GetFile(string virtualPath)
        {
            if (IsDoItPath(virtualPath))
                return new TemplateVirtualFile(virtualPath);

            return Previous.GetFile(virtualPath);
        }

        public override VirtualDirectory GetDirectory(string virtualDir)
        {
            //if (IsDoItPath(virtualDir))
            //    return new TemplateVirtualDirectory(virtualDir);

            return Previous.GetDirectory(virtualDir);
        }
     }
}
