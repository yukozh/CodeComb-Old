using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.IO;
using Microsoft.AspNet.SignalR;
using PushSharp.Apple;
using PushSharp.Core;
using PushSharp;

namespace CodeComb.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            GlobalHost.Configuration.ConnectionTimeout = TimeSpan.FromHours(24);
            GlobalHost.Configuration.TransportConnectTimeout = TimeSpan.FromHours(24);
            //Helpers.Push.push.RegisterAppleService(new ApplePushChannelSettings(File.ReadAllBytes(@"C:\CodeCombCert\ios_push_sandbox.p12"), "CodeComb"));
        }
    }
}
