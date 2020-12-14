using System.IO;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using MimeTypes;
using Dnote.H5P.Consts;
using Dnote.H5P.Helpers;

namespace Dnote.H5P
{
    public class H5PAzureMetaDataAgent : H5PFileMetaDataAgent
    {
        private readonly string _container;
        private readonly CloudStorageAccount _storageAccount;

        public H5PAzureMetaDataAgent(string connectionString, string container)
            : base(null)
        {
            _container = container;
            _storageAccount = CloudStorageAccount.Parse(connectionString);
        }

        protected override string? GetFileSystemPrefix()
        {
            return _storageAccount.BlobStorageUri.PrimaryUri.ToString().TrimEnd('/') + "/" + _container + "/";
        }

        protected override string GetMetaDataPath(string contentId)
        {
            return Path.Combine("content", contentId, "metadata.json");
        }

        protected async override Task<string?> ReadContentAsync(string path)
        {
            var blobClient = _storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(_container);
            var containerExists = await container.ExistsAsync();
            if (!containerExists)
            {
                return null;
            }

            var blockBlob = container.GetBlockBlobReference(path);
            var blobExists = await blockBlob.ExistsAsync();
            if (!blobExists)
            {
                return null;
            }

            return await blockBlob.DownloadTextAsync();
        }

        protected async override Task StoreContentAsync(string path, string value)
        {
            var blobClient = _storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(_container);
            var blockBlob = container.GetBlockBlobReference(path);
            blockBlob.Properties.ContentType = MimeTypeMap.GetMimeType(Path.GetExtension(path));

            // set browser caching on the media blobs
            if (PathHelper.IsCacheableFileType(path))
            {
                blockBlob.Properties.CacheControl = $"public, max-age={H5PConsts.MediaCacheDurationSecs}";
            }

            await blockBlob.UploadTextAsync(value);
        }
    }
}
