using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using WebApplication1.DataAccess;

namespace WebApplication1
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
         

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);


            System.Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS",
                //Server.MapPath("progforthecloudt2020-63d67543309b.json")
                Server.MapPath("justinportellimsd63b-e10d16588fb7.json")
                );



            //   var cipher = KeyRepository.Encrypt("hello world");


            //new LogsRepository().WriteLogEntry("App starting...");
        }
    }
}
