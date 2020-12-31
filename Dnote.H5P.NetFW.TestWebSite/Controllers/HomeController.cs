#nullable enable
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Azure.Storage.Blobs;
using Dnote.H5P.NetFW.TestWebSite.Enums;
using Dnote.H5P.NetFW.TestWebSite.Models;

namespace Dnote.H5P.NetFW.TestWebSite.Controllers
{
    public class HomeController : Controller
    {
        private readonly BlobServiceClient _blobServiceClient;

        public HomeController()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["StorageConnection"].ConnectionString;
            _blobServiceClient = new BlobServiceClient(connectionString);
        }

        [HttpGet]
        public async Task<ActionResult> Index(StorageEnum storage = StorageEnum.File)
        {
            var model = new IndexViewModel
            {
                Storage = storage
            };

            model.ContentItems = await GetContentItems(storage);

            model.Storages = Enum.GetValues(typeof(StorageEnum)).Cast<StorageEnum>().Select(v => new SelectListItem
            {
                Text = v.ToString(),
                Value = v.ToString()
            }).ToList();

            return View(model);
        }

        private async Task<IEnumerable<string>> GetContentItems(StorageEnum storage)
        {
            switch (storage)
            {
                case StorageEnum.File:
                    return GetContentItemsFile();
                case StorageEnum.Azure:
                    return await GetContentItemsAzure();
                default:
                    throw new ArgumentOutOfRangeException(nameof(storage));
            }
        }

        private IEnumerable<string> GetContentItemsFile()
        {
            var dirs = new string[0];

            var dir = Server.MapPath("~/Content/content");
            if (Directory.Exists(dir))
            {
                dirs = Directory.GetDirectories(dir);
            }

            return dirs.Select(d => Path.GetFileNameWithoutExtension(d));
        }

        private async Task<IEnumerable<string>> GetContentItemsAzure()
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient("h5ptest");
            var containerExists = await containerClient.ExistsAsync();
            if (containerExists)
            {
                var dirBlobs = new List<string>();
                var contentDir = containerClient.GetBlobsByHierarchyAsync(prefix: "content/", delimiter: "/");
                await foreach (var item in contentDir)
                {
                    if (item.IsPrefix)
                    {
                        dirBlobs.Add(Path.GetFileName(item.Prefix.TrimEnd('/')));
                    }
                }
                return dirBlobs;
            }

            return new string[0];
        }

        [HttpPost]
        public ActionResult Index(HttpPostedFileBase file, IndexViewModel model)
        {
            if (file != null)
            {
                var storageAgent = GetStorageAgent(model.Storage);
                var metaDataAgent = GetMetaDataAgent(model.Storage);

                var importer = new H5PImporter(storageAgent, metaDataAgent);

                importer.Import(file.InputStream, file.FileName.Replace('.', '-').Replace(' ', '-'));
            }

            return Redirect(Url.Action(null, new { model.Storage }));
        }

        private H5PFileMetaDataAgent GetMetaDataAgent(StorageEnum storage)
        {
            switch (storage)
            {
                case StorageEnum.File:
                    var storagePath = Server.MapPath("~/Content");
                    return new H5PPhysicalFileMetaDataAgent(Url.Content("~/Content"), storagePath);
                case StorageEnum.Azure:
                    var connectionString = ConfigurationManager.ConnectionStrings["StorageConnection"].ConnectionString;
                    return new H5PAzureMetaDataAgent(  connectionString, "h5ptest");
                default:
                    throw new ArgumentOutOfRangeException(nameof(storage));
            }
        }

        private H5PStorageAgent GetStorageAgent(StorageEnum storage)
        {
            switch (storage)
            {
                case StorageEnum.File:
                    var storagePath = Server.MapPath("~/Content");
                    return new H5PFileStorageAgent(storagePath);
                case StorageEnum.Azure:
                    var connectionString = ConfigurationManager.ConnectionStrings["StorageConnection"].ConnectionString;
                    return new H5PAzureStorageAgent(connectionString, "h5ptest");
                default:
                    throw new ArgumentOutOfRangeException(nameof(storage));
            }
        }
        
        [HttpGet]
        public ActionResult Exercise(string id, StorageEnum storage)
        {
            var metaDataAgent = GetMetaDataAgent(storage);

            metaDataAgent.LoadContent(new[] { id });

            var userState = (string?)Session[id];

            // An empty string user state means the content item has been submitted, but the content type does not support state.
            var visible = userState != "";

            var contentItem = metaDataAgent.GetContentItem(id);

            var model = new ExerciseViewModel
            {
                Id = id,
                Visible = visible,
                Title = contentItem.Title,
                H5PMetaDataAgent = metaDataAgent,
                State = userState,
                Storage = storage
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult Exercise(string id, ExerciseViewModel model)
        {
            // For demo purposes store the state in the session. In real life you would save it in the database.
            Session[id] = model.State ?? "";

            return Redirect(Url.Action(null, new { id, model.Storage }));
        }
    }
}