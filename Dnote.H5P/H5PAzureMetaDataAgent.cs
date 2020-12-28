using System.IO;
using System.Text;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using MimeTypes;
using Dnote.H5P.Consts;
using Dnote.H5P.Helpers;

namespace Dnote.H5P
{
    public class H5PAzureMetaDataAgent : H5PFileMetaDataAgent
    {
        private readonly string _container;
        private readonly BlobServiceClient _blobServiceClient;

        public H5PAzureMetaDataAgent(string connectionString, string container)
            : base(null)
        {
            _container = container;
            _blobServiceClient = new BlobServiceClient(connectionString);
        }

        protected override string? GetFileSystemPrefix()
        {
            return _blobServiceClient.Uri.ToString().TrimEnd('/') + "/" + _container + "/";
        }

        protected override string GetMetaDataPath(string contentId)
        {
            return Path.Combine("content", contentId, "metadata.json");
        }

        protected override string? ReadContent(string path)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_container);
            var containerExists = containerClient.Exists();
            if (!containerExists)
            {
                return null;
            }

            var blobClient = containerClient.GetBlobClient(path);
            var blobExists = blobClient.Exists();
            if (!blobExists)
            {
                return null;
            }

            var download = blobClient.Download();

            using var reader = new StreamReader(download.Value.Content);
            return reader.ReadToEnd();
        }

        protected override void StoreContent(string path, string value)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_container);
            var containerExists = containerClient.Exists();
            if (!containerExists)
            {
                _blobServiceClient.CreateBlobContainer(_container, PublicAccessType.Blob);
            }

            var blockBlob = containerClient.GetBlobClient(path);
            var blobHttpHeader = new BlobHttpHeaders
            {
                ContentType = MimeTypeMap.GetMimeType(Path.GetExtension(path))
            };

            // set browser caching on the media blobs
            if (PathHelper.IsCacheableFileType(path))
            {
                blobHttpHeader.CacheControl = $"public, max-age={H5PConsts.MediaCacheDurationSecs}";
            }

            var stream = new MemoryStream(Encoding.UTF8.GetBytes(value));
            blockBlob.Upload(stream, blobHttpHeader);
        }
    }
}
