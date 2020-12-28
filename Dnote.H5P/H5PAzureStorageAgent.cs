using System.IO;
using System.Threading.Tasks;
using MimeTypes;
using Dnote.H5P.Consts;
using Dnote.H5P.Helpers;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Dnote.H5P
{
    public class H5PAzureStorageAgent : H5PStorageAgent
    {
        private readonly string _container;
        private readonly BlobServiceClient _blobServiceClient;

        public H5PAzureStorageAgent(string connectionString, string container)
        {
            _container = container;
            _blobServiceClient = new BlobServiceClient(connectionString);
        }

        public override void StoreFile(Stream stream, string fileName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_container);
            var containerExists = containerClient.Exists();
            if (!containerExists)
            {
                _blobServiceClient.CreateBlobContainer(_container, PublicAccessType.Blob);
            }

            var blockBlob = containerClient.GetBlobClient(fileName);
            var blobHttpHeader = new BlobHttpHeaders
            {
                ContentType = MimeTypeMap.GetMimeType(Path.GetExtension(fileName))
            };

            // set browser caching on the media blobs
            if (PathHelper.IsCacheableFileType(fileName))
            {
                blobHttpHeader.CacheControl = $"public, max-age={H5PConsts.MediaCacheDurationSecs}";
            }

            blockBlob.Upload(stream, blobHttpHeader);
        }
    }
}
