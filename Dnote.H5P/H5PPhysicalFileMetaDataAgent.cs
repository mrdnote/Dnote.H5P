using System.IO;
using System.Threading.Tasks;

namespace Dnote.H5P
{
    public class H5PPhysicalFileMetaDataAgent : H5PFileMetaDataAgent
    {
        private readonly string _filePath;

        public H5PPhysicalFileMetaDataAgent(string pathPrefix, string filePath)
            : base(pathPrefix)
        {
            _filePath = filePath;
        }

        protected override Task<string?> ReadContentAsync(string path)
        {
            string? result = null;

            if (File.Exists(path))
            {
                result = File.ReadAllText(path);
            }

            return Task.FromResult(result);
        }

        protected override Task StoreContentAsync(string path, string value)
        {
            File.WriteAllText(path, value);
            return Task.CompletedTask;
        }

        protected override string GetMetaDataPath(string contentId)
        {
            return Path.Combine(_filePath, "content", contentId, "metadata.json");
        }

        protected override string? GetFileSystemPrefix()
        {
            return null;
        }
    }
}
