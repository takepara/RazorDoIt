using System;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
using System.Threading;

namespace RazorDoIt
{
    public class InAppDomain
    {
        public static AppDomain Create()
        {
            AppDomainSetup ads = new AppDomainSetup();
            ads.DisallowBindingRedirects = false;
            ads.DisallowCodeDownload = true;
            ads.PartialTrustVisibleAssemblies = new[]
            {
                "System.ComponentModel.DataAnnotations," + 
                "PublicKey=0024000004800000940000000602000000240000525341310004000001000100B5FC90E7027F67871E773A8FDE8938C81DD402BA65B9201D60593E96C492651E889CC13F1415EBB53FAC1131AE0BD333C5EE6021672D9718EA31A8AEBD0DA0072F25D87DBA6FC90FFD598ED4DA35E44C398C454307E8E33B8426143DAEC9F596836F97C8F74750E5975C64E2189F45DEF46B2A2B1247ADC3652BF5C308055DA9"
            };
            var evidence = new Evidence();
            //evidence.AddHostEvidence(new Zone(SecurityZone.Internet));
            //PermissionSet permissions = new NamedPermissionSet("Internet", SecurityManager.GetStandardSandbox(evidence));
            //ads.ApplicationTrust = new ApplicationTrust(permissions, new[] { typeof(CSharpCodeProvider).Assembly });

            var dir = @"C:\Users\takehara\Documents\My SkyDrive\My OpenSources\RazorDoIt\RazorDoIt.Sandbox";
            ads.ShadowCopyDirectories = dir + "\\bin";
            ads.PrivateBinPath = dir;
            ads.ApplicationBase = dir;
            ads.ConfigurationFile = dir + "\\web.config";
            //Path.Combine(dir, "..\\Core\\web.config");

            //ads.PrivateBinPath = Path.Combine(dir, "..\\Core\\bin");
            //ads.ApplicationBase = Path.Combine(dir, "..\\Core\\bin");
            //var permissions = new PermissionSet(PermissionState.None);
            //permissions.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
            var permissions = new PermissionSet(SecurityManager.GetStandardSandbox(evidence));
            //permissions.AddPermission(new ReflectionPermission(ReflectionPermissionFlag.MemberAccess));
            permissions.AddPermission(new FileIOPermission(PermissionState.Unrestricted));
            permissions.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
            //permissions.AddPermission(new  (SecurityPermissionFlag.Execution));
            //permissions.RemovePermission(typeof())   
            var name = "DoIt." + Guid.NewGuid().ToString("N");
            var appDomain = AppDomain.CreateDomain(name, evidence, ads/*, permissions*/);
            //foreach (var asmName in referenceAssemblies)
            //{
            //    appDomain.Load(asmName);
            //}
            return appDomain;
        }

        public static T Run<T>(Func<AppDomain, T> functor)
        {
            T result;
            var appDomain = Create();
            //AppDomain appDomain = null;
            try
            {
                result = functor(appDomain);
            }
            finally
            {
                ThreadPool.QueueUserWorkItem((_) =>
                {
                    AppDomain.Unload(appDomain);
                });
            }
            return result;
        }

    }
}
