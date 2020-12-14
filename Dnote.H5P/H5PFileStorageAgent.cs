using System.IO;
using System.Threading.Tasks;

namespace Dnote.H5P
{
    public class H5PFileStorageAgent : H5PStorageAgent
    {
        private readonly string _path;

        public H5PFileStorageAgent(string path)
        {
            _path = path;
        }

        public override async Task StoreFileAsync(Stream stream, string fileName)
        {
            var filePath = Path.Combine(_path, fileName);
            var filePathDir = Path.GetDirectoryName(filePath);
            Directory.CreateDirectory(filePathDir);

            using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            await stream.CopyToAsync(fileStream);
        }
    }
}
