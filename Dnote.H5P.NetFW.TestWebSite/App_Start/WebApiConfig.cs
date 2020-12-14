using Dnote.H5P.NetFW.TestWebSite.Controllers;
using System.Linq;
using System.Web.Http;

namespace Dnote.H5P.NetFW.TestWebSite.App_Start
{
    public class WebApiConfig
    {
        public const string UrlPrefix = "api";
        public const string DefaultApiRouteName = "DefaultApi";
        public const string H5PUrlPrefix = "h5papi";
        public const string DefaultH5PApiRouteName = "H5PApi";

        public static void Register(HttpConfiguration config)
        {
            config.EnableCors();

            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                DefaultApiRouteName,
                UrlPrefix + "/{controller}/{action}/{id}",
                new { action = RouteParameter.Optional, id = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                DefaultH5PApiRouteName,
                H5PUrlPrefix + "/{action}/{id}",
                new { controller = nameof(H5PApiController).Replace("Controller", null), id = RouteParameter.Optional }
            );

            // force json
            var appXmlType = config.Formatters.XmlFormatter.SupportedMediaTypes.FirstOrDefault(t => t.MediaType == "application/xml");
            config.Formatters.XmlFormatter.SupportedMediaTypes.Remove(appXmlType);
        }
    }
}