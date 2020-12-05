using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Dnote.H5P.Dto;

namespace Dnote.H5P
{
    public class H5PFileMetaDataAgent : H5PMetaDataAgent
    {
        private readonly List<H5PContentItemFileDto> _fileDtos = new List<H5PContentItemFileDto>();

        private readonly string _filePath;

        public H5PFileMetaDataAgent(string pathPrefix, string filePath)
            : base(pathPrefix)
        {
            _filePath = filePath;
        }

        protected override void InnerLoadContent(IEnumerable<string> contentIds)
        {
            _fileDtos.Clear();

            foreach (var contentId in contentIds)
            {
                var metaDataPath = GetMetaDataPath(contentId);
                H5PContentItemFileDto fileDto;

                if (File.Exists(metaDataPath))
                {
                    var jsonString = File.ReadAllText(metaDataPath);

                    fileDto = JsonConvert.DeserializeObject<H5PContentItemFileDto>(jsonString);
                }
                else
                {
                    fileDto = new H5PContentItemFileDto
                    {
                        ContentId = contentId
                    };
                }

                _fileDtos.Add(fileDto);
            }
        }

        protected override void InnerStoreContentItem(H5PJsonDto h5pJson, string contentId, string content)
        {
            var fileDto = _fileDtos.FirstOrDefault(f => f.ContentId == contentId);
            
            fileDto.Title = h5pJson.Title;
            fileDto.Content = content;
        }

        public override void SaveContent()
        {
            foreach (var fileDto in _fileDtos)
            {
                var metaDataPath = GetMetaDataPath(fileDto.ContentId);

                var jsonString = JsonConvert.SerializeObject(fileDto);

                File.WriteAllText(metaDataPath, jsonString);
            }
        }

        protected override void UpdateLibraryInContent(string contentId, string machineName, int majorVersion, int minorVersion, IEnumerable<string>? jsFiles, IEnumerable<string>? cssFiles, int order,
            bool isMainLibrary)
        {
            var fileDto = _fileDtos.First(f => f.ContentId == contentId);

            var library = fileDto.Libraries.FirstOrDefault(l => l.MachineName == machineName);

            if (library != null && (library.MajorVersion != majorVersion || library.MinorVersion != minorVersion))
            {
                fileDto.Libraries.Remove(library);
                library = null;
            }

            if (library == null)
            {
                library = new H5PContentItemFileDto.Library
                {
                    MachineName = machineName,
                    MajorVersion = majorVersion,
                    MinorVersion = minorVersion
                };
                fileDto.Libraries.Add(library);
            }

            library.JsFiles = jsFiles ?? new string[0];
            library.CssFiles = cssFiles ?? new string[0];
            library.Order = order;

            if (isMainLibrary)
            {
                fileDto.MainLibrary = library;
            }
        }

        protected override IEnumerable<H5PContentItemFileDto> InnerGetContentItems()
        {
            return _fileDtos;
        }

        protected override IEnumerable<H5PLibraryForContentItemDto> GetLibrariesForContentItems()
        {
            var contentLibraries = _fileDtos.SelectMany(fd => fd.Libraries).Distinct().OrderBy(l => l.Order);

            return contentLibraries.Select(l => new H5PLibraryForContentItemDto
            {
                MachineName = l.MachineName,
                MajorVersion = l.MajorVersion,
                MinorVersion = l.MinorVersion,
                JsFiles = l.JsFiles,
                CssFiles = l.CssFiles
            });
        }

        protected override void SaveLibrary(string title, string machineName, int majorVersion, int minorVersion, int patchVersion, int? coreApiMajorVersion, int? coreApiMinorVersion, string author, 
            IEnumerable<string>? jsFiles, IEnumerable<string>? cssFiles)
        {
        }

        private string GetMetaDataPath(string contentId)
        {
            return Path.Combine(_filePath, "content", contentId, "metadata.json");
        }
    }
}
