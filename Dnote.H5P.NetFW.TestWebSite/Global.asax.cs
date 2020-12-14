using System.Web.Mvc;
using System.Web.Http;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web;
using System.Web.SessionState;
using Dnote.H5P.NetFW.TestWebSite.App_Start;

namespace Dnote.H5P.TestWebSiteFW
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_PostAuthorizeRequest()
        {
            if (IsWebApiRequest())
            {
                HttpContext.Current.SetSessionStateBehavior(SessionStateBehavior.Required);
            }
        }

        private bool IsWebApiRequest()
        {
            return
                HttpContext.Current.Request.AppRelativeCurrentExecutionFilePath.StartsWith("~/" + WebApiConfig.UrlPrefix)
                ||
                HttpContext.Current.Request.AppRelativeCurrentExecutionFilePath.StartsWith("~/" + WebApiConfig.H5PUrlPrefix);
        }
    }
}
