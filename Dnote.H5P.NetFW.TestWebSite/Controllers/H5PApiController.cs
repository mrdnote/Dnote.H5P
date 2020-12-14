using Dnote.H5P.NetFW.Api;
using System.Web;

namespace Dnote.H5P.NetFW.TestWebSite.Controllers
{
    public class H5PApiController : H5PApiControllerBase
    {
        protected override void InnerSetFinished(string contentId, int opened, int finished, double score, double maxScore)
        {
            var s = Request.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        }

        protected override void InnerStoreUserData(string contentId, string dataType, string subContentId, string data)
        {
            HttpContext.Current.Session[contentId] = data;
        }
    }
}