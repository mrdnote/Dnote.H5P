using System.IO;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using MimeTypes;
using Dnote.H5P.Consts;
using Dnote.H5P.Helpers;

namespace Dnote.H5P
{
    public class H5PAzureStorageAgent : H5PStorageAgent
    {
        private readonly string _container;
        private readonly CloudStorageAccount _storageAccount;

        public H5PAzureStorageAgent(string connectionString, string container)
        {
            _container = container;
            _storageAccount = CloudStorageAccount.Parse(connectionString);
        }

        public async override Task StoreFileAsync(Stream stream, string fileName)
        {
            var blobClient = _storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(_container);
            var exists = await container.ExistsAsync();
            if (!exists)
            {
                await container.CreateIfNotExistsAsync();
                await container.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });
            }

            var blockBlob = container.GetBlockBlobReference(fileName);
            blockBlob.Properties.ContentType = MimeTypeMap.GetMimeType(Path.GetExtension(fileName));

            // set browser caching on the media blobs
            if (PathHelper.IsCacheableFileType(fileName))
            {
                blockBlob.Properties.CacheControl = $"public, max-age={H5PConsts.MediaCacheDurationSecs}";
            }

            await blockBlob.UploadFromStreamAsync(stream);
        }
    }
}
