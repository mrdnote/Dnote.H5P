#nullable enable
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Dnote.H5P.NetFW.TestWebSite.Enums;
using Dnote.H5P.NetFW.TestWebSite.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Dnote.H5P.NetFW.TestWebSite.Controllers
{
    public class HomeController : Controller
    {
        private readonly CloudStorageAccount _storageAccount;

        public HomeController()
        {
            _storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["StorageConnection"].ConnectionString);
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
            var blobClient = _storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference("h5ptest");
            var exists = await container.ExistsAsync();
            if (exists)
            {
                var contentDir = container.GetDirectoryReference("content");
                var dirBlobs = contentDir.ListBlobs().Where(b => b as CloudBlobDirectory != null).ToList();
                return dirBlobs.Select(db => db.Uri.Segments.LastOrDefault().TrimEnd('/'));
            }

            return new string[0];
        }

        [HttpPost]
        public async Task<ActionResult> Index(HttpPostedFileBase file, IndexViewModel model)
        {
            if (file != null)
            {
                var storageAgent = GetStorageAgent(model.Storage);
                var metaDataAgent = GetMetaDataAgent(model.Storage);

                var importer = new H5PImporter(storageAgent, metaDataAgent);

                await importer.Import(file.InputStream, file.FileName.Replace('.', '-').Replace(' ', '-'));
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
        public async Task<ActionResult> Exercise(string id, StorageEnum storage)
        {
            var metaDataAgent = GetMetaDataAgent(storage);

            await metaDataAgent.LoadContentAsync(new[] { id });

            var userState = (string?)Session[id];

            // An empty string user state means the content item has been submitted, but the content type does not support state.
            var visible = userState != "";
            metaDataAgent.SetVisiblity(id, visible);
            metaDataAgent.SetUserState(id, userState);

            var contentItem = metaDataAgent.GetContentItem(id);

            var model = new ExerciseViewModel
            {
                Id = id,
                Visible = visible,
                Title = contentItem.Title,
                H5PMetaDataAgent = metaDataAgent,
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