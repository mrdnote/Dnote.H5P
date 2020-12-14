using System.Net;
using System.Net.Http;
using System.Web.Http;
using Dnote.H5P.NetFW.Api.Model;

namespace Dnote.H5P.NetFW.Api
{
    public abstract class H5PApiControllerBase : ApiController
    {
        [HttpPost]
        public HttpResponseMessage SetFinished([FromBody] SetFinishedBodyModel model)
        {
            var s = Request.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            InnerSetFinished(model.ContentId, model.Opened, model.Finished, model.Score, model.MaxScore);

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        protected abstract void InnerSetFinished(string contentId, int opened, int finished, double score, double maxScore);

        [HttpPost]
        public HttpResponseMessage UserData(string id, string data_type, string subContentId, [FromBody] UserDataBodyModel model)
        {
            var data = model.Data;

            InnerStoreUserData(id, data_type, subContentId, data);

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        protected abstract void InnerStoreUserData(string id, string dataType, string subContentId, string data);
    }
}
