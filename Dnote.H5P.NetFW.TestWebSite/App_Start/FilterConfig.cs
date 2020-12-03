using System.Web.Mvc;

namespace Dnote.H5P.NetFW.TestWebSite.App_Start
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
