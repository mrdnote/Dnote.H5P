using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Dnote.H5P.Dto;

namespace Dnote.H5P
{
    public abstract class H5PFileMetaDataAgent : H5PMetaDataAgent
    {
        private readonly List<H5PContentItemDto> _fileDtos = new List<H5PContentItemDto>();

        public H5PFileMetaDataAgent(string? pathPrefix)
            : base(pathPrefix)
        {
        }

        protected override void InnerLoadContent(IEnumerable<string> contentIds)
        {
            _fileDtos.Clear();

            foreach (var contentId in contentIds)
            {
                var metaDataPath = GetMetaDataPath(contentId);
                H5PContentItemDto fileDto;

                var jsonString = ReadContent(metaDataPath);

                if (jsonString != null)
                {
                    fileDto = JsonConvert.DeserializeObject<H5PContentItemDto>(jsonString);
                }
                else
                {
                    fileDto = new H5PContentItemDto
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

                StoreContent(metaDataPath, jsonString);
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
                library = new H5PContentItemDto.Library
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

        protected override IEnumerable<H5PContentItemDto> InnerGetContentItems()
        {
            return _fileDtos;
        }

        protected override H5PContentItemDto InnerGetContentItem(string contentId)
        {
            return _fileDtos.FirstOrDefault(f => f.ContentId == contentId);
        }

        protected override IEnumerable<H5PLibraryForContentItemDto> GetLibrariesForContentItem(string contentId)
        {
            var fileDto = _fileDtos.FirstOrDefault(f => f.ContentId == contentId);
            var contentLibraries = fileDto.Libraries.OrderBy(l => l.Order);

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

        protected abstract string GetMetaDataPath(string contentId);

        /// <summary>
        /// Reads the content of the specified file as a string. Returns null if the file does not exist.
        /// </summary>
        protected abstract string? ReadContent(string path);

        protected abstract void StoreContent(string path, string value);
    }
}
