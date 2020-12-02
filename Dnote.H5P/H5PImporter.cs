using System.IO;
using System.IO.Compression;
using System.Text;
using System.Linq;
using Newtonsoft.Json;
using Dnote.H5P.Dto;

namespace Dnote.H5P
{
    public class H5PImporter
    {
        private readonly H5PStorageAgent _storageAgent;
        private readonly H5PMetaDataAgent _metaDataAgent;

        public H5PImporter(H5PStorageAgent storageAgent, H5PMetaDataAgent databaseAgent)
        {
            _storageAgent = storageAgent;
            _metaDataAgent = databaseAgent;
        }

        public void Import(string fileName)
        {
            using var fileStream = new FileStream(fileName, FileMode.Open);
            Import(fileStream, Path.GetFileName(fileName));
        }

        public void Import(Stream? stream, string contentId)
        {
            using var zip = new ZipArchive(stream);
            Import(zip, contentId);
        }

        public void Import(ZipArchive zip, string contentId)
        {
            var h5pjsonEntry = zip.GetEntry("h5p.json");
            using var h5pjsonStream = new StreamReader(h5pjsonEntry.Open(), Encoding.Default);
            var h5pjsonString = h5pjsonStream.ReadToEnd();
            var h5pJson = JsonConvert.DeserializeObject<H5PJsonDto>(h5pjsonString);

            // Read H5P content to in-memory objects and also store resources in cloud or filesystem.
            if (h5pJson.PreloadedDependencies != null)
            {
                foreach (var dependency in h5pJson.PreloadedDependencies)
                {
                    var libraryJson = ImportDependencyLibrary(dependency, zip, h5pJson);
                    dependency.Library = libraryJson;
                }
            }

            // Store dependency meta data in database or whatever storage metadata agent prefers.
            _metaDataAgent.StoreContent(h5pJson, contentId);
        }

        private H5PJsonDto.Library ImportDependencyLibrary(H5PJsonDto.Dependency dependency, ZipArchive zip, H5PJsonDto h5pJson)
        {
            var dirName = $"{dependency.MachineName}-{dependency.MajorVersion}.{dependency.MinorVersion}";

            // TODO: only proceed if patch version is higher than stored patch version

            var libraryEntry = zip.GetEntry(dirName + "/" + "library.json");
            using var libraryStream = new StreamReader(libraryEntry.Open(), Encoding.Default);
            var libraryString = libraryStream.ReadToEnd();
            var libraryJson = JsonConvert.DeserializeObject<H5PJsonDto.Library>(libraryString);

            if (libraryJson.PreloadedDependencies != null)
            {
                foreach (var libraryDependency in libraryJson.PreloadedDependencies)
                {
                    var rootDependency = h5pJson.PreloadedDependencies.FirstOrDefault(d => d.MachineName == libraryDependency.MachineName && d.MajorVersion == libraryDependency.MajorVersion && d.MinorVersion == libraryDependency.MinorVersion);

                    // If the dependency exists in the root of the file, use it to fulfill this nested dependency
                    if (rootDependency?.Library != null)
                    {
                        libraryDependency.Library = rootDependency.Library;
                    }
                    else // import it on the spot
                    {
                        var library = ImportDependencyLibrary(libraryDependency, zip, h5pJson);
                        libraryDependency.Library = library;

                        // also save the library to the root dependency, if it exists in the root
                        if (rootDependency != null)
                        {
                            rootDependency.Library = library;
                        }
                    }
                }
            }

            var entries = zip.Entries.Where(e => e.FullName.StartsWith(dirName.TrimEnd('/') + "/"));
            foreach (var entry in entries)
            {
                StoreLibraryFile(entry.FullName, zip);
            }

            return libraryJson;
        }

        private void StoreLibraryFile(string fileName, ZipArchive zip)
        {
            var entry = zip.GetEntry(fileName);
            using var stream = entry.Open();
            _storageAgent.StoreFile(stream, fileName);
        }
    }
}
