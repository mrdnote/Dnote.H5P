#nullable enable
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dnote.H5P.NetFW.Linq2Sql;
using Dnote.H5P.NetFW.TestWebSite.Models;

namespace Dnote.H5P.NetFW.TestWebSite.Controllers
{
    public class HomeController : Controller
    {
        private readonly H5PDataContext _context;

        public HomeController()
        {
            _context = new H5PDataContext(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
        }

        [HttpGet]
        public ActionResult Index()
        {
            var model = new IndexViewModel();

            model.ContentItems = _context.Repository<H5P_ContentItem>().OrderBy(c => c.Title).ToDictionary(c => c.ContentId, c => c.Title);

            return View(model);
        }

        [HttpPost]
        public ActionResult Index(/*IndexViewModel model, */ HttpPostedFileBase file)
        {
            if (file != null)
            {
                var storageAgent = new H5PFileStorageAgent(Server.MapPath("~/Content"));
                var metaDataAgent = new H5PLinqMetaDataAgent(_context, Url.Content("~/Content"));
                var importer = new H5PImporter(storageAgent, metaDataAgent);
                importer.Import(file.InputStream, file.FileName.Replace('.', '-').Replace(' ', '-'));
            }

            return Redirect("/");
        }

        public ActionResult Exercise(string id)
        {
            var contentItem = _context.Repository<H5P_ContentItem>().First(c => c.ContentId == id);

            var h5pMetaDataAgent = new H5PLinqMetaDataAgent(_context, Url.Content("~/Content"));

            var model = new ExerciseViewModel
            {
                Id = id,
                Title = contentItem.Title,
                H5PMetaDataAgent = h5pMetaDataAgent
            };

            return View(model);
        }
    }
}