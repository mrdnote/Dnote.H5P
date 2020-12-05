#nullable enable
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dnote.H5P.NetFW.TestWebSite.Models;

namespace Dnote.H5P.NetFW.TestWebSite.Controllers
{
    public class HomeController : Controller
    {
        public HomeController()
        {
        }

        [HttpGet]
        public ActionResult Index()
        {
            var model = new IndexViewModel();

            var dirs = new string[0];

            var dir = Server.MapPath("~/Content/content");
            if (Directory.Exists(dir))
            {
                dirs = Directory.GetDirectories(dir);
            }

            model.ContentItems = dirs.Select(d => Path.GetFileNameWithoutExtension(d));

            return View(model);
        }

        [HttpPost]
        public ActionResult Index(/*IndexViewModel model, */ HttpPostedFileBase file)
        {
            if (file != null)
            {
                var storagePath = Server.MapPath("~/Content");
                var storageAgent = new H5PFileStorageAgent(storagePath);
                var metaDataAgent = new H5PFileMetaDataAgent(Url.Content("~/Content"), storagePath);
                var importer = new H5PImporter(storageAgent, metaDataAgent);

                importer.Import(file.InputStream, file.FileName.Replace('.', '-').Replace(' ', '-'));
            }

            return Redirect("/");
        }

        public ActionResult Exercise(string id)
        {
            var storagePath = Server.MapPath("~/Content");
            var metaDataAgent = new H5PFileMetaDataAgent(Url.Content("~/Content"), storagePath);

            metaDataAgent.LoadContent(new[] { id });

            var contentItem = metaDataAgent.GetContentItem(id);

            var model = new ExerciseViewModel
            {
                Id = id,
                Title = contentItem.Title,
                H5PMetaDataAgent = metaDataAgent
            };

            return View(model);
        }
    }
}