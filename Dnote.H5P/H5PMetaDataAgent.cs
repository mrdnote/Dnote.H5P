using System;
using System.Collections.Generic;
using System.Linq;
using Dnote.H5P.Dto;
using Dnote.H5P.Enums;

namespace Dnote.H5P
{
    /// <summary>
    /// Base class for the agent that stores the H5P type meta data. This meta data is used to render the content and all its dependencies later on.
    /// </summary>
    public abstract class H5PMetaDataAgent
    {
        private readonly string _pathPrefix;

        public H5PMetaDataAgent(string pathPrefix)
        {
            _pathPrefix = pathPrefix;
        }

        public void StoreContent(H5PJsonDto h5pJson, string contentId)
        {
            SaveContent(contentId, h5pJson.Title, h5pJson.Language, h5pJson.License, h5pJson.DefaultLanguage, h5pJson.EmbedTypes);

            SaveChanges();

            var touchedLibraries = new List<(string, int, int)>();

            if (h5pJson.PreloadedDependencies != null)
            {
                SaveContentDependencies(h5pJson.PreloadedDependencies, contentId, touchedLibraries);
            }
        }

        private void SaveContentDependencies(IEnumerable<H5PJsonDto.Dependency> dependencies, string contentId, List<(string, int, int)> touchedLibraries)
        {
            foreach (var dependency in dependencies)
            {
                ProcessLibrary(dependency.Library, contentId, touchedLibraries);
            }
        }

        private void ProcessLibrary(H5PJsonDto.Library library, string contentId, List<(string, int, int)> touchedLibraries)
        {
            var dependencies = library.PreloadedDependencies;

            if (dependencies != null)
            {
                foreach (var dependency in dependencies)
                {
                    ProcessLibrary(dependency.Library, contentId, touchedLibraries);
                }
            }

            SaveLibrary(library.Title, library.MachineName, library.MajorVersion, library.MinorVersion, library.PatchVersion, library.CoreApi?.MajorVersion, library.CoreApi?.MinorVersion, 
                library.Author, library.PreloadedJs?.Select(f => f.Path), library.PreloadedCss?.Select(f => f.Path));

            SaveChanges();

            if (!touchedLibraries.Any(l => l.Item1 == library.MachineName && l.Item2 == library.MajorVersion && l.Item3 == library.MinorVersion))
            {
                if (!LibraryExistsInContent(contentId, library.MachineName, library.MajorVersion, library.MinorVersion))
                {
                    AddLibraryToContent(contentId, library.MachineName, library.MajorVersion, library.MinorVersion, touchedLibraries.Count);
                }
                else
                {
                    SetLibraryOrderInContent(contentId, library.MachineName, library.MajorVersion, library.MinorVersion, touchedLibraries.Count);
                }
                touchedLibraries.Add((library.MachineName, library.MajorVersion, library.MinorVersion));
            }

            SaveChanges();
        }

        public IEnumerable<string> GetIncludeFilesForContentItems(IEnumerable<string> contentIds, FileTypes fileType)
        {
            var libraries = GetLibrariesForContentItems(contentIds);

            var hasVersionConflicts = libraries.GroupBy(l => new { l.MachineName, l.MajorVersion, l.MinorVersion }).Any(g => g.Count() > 1);

            if (hasVersionConflicts)
            {
                throw new Exception("The specified content items are depended on different versions of the same library!");
            }

            var pathPrefix = _pathPrefix;

            if (pathPrefix != null && pathPrefix != "")
            {
                pathPrefix = pathPrefix.TrimEnd('/') + "/";
            }

            if (fileType == FileTypes.Js)
            {
                return libraries.SelectMany(l => l.JsFiles.Select(f => $"{pathPrefix}{l.MachineName}-{l.MajorVersion}.{l.MinorVersion}/{f}")).Distinct();
            }
            else if (fileType == FileTypes.Css)
            {
                return libraries.SelectMany(l => l.CssFiles.Select(f => $"{pathPrefix}{l.MachineName}-{l.MajorVersion}.{l.MinorVersion}/{f}")).Distinct();
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(fileType));
            }
        }

        protected abstract void SaveContent(string contentId, string title, string language, string license, string defaultLanguage, IEnumerable<string> embedTypes);

        protected abstract void SaveLibrary(string title, string machineName, int majorVersion, int minorVersion, int patchVersion, int? coreApiMajorVersion, int? coreApiMinorVersion, 
            string author, IEnumerable<string>? jsFiles, IEnumerable<string>? cssFiles);

        protected abstract bool LibraryExistsInContent(string contentId, string machineName, int majorVersion, int minorVersion);

        protected abstract void AddLibraryToContent(string contentId, string machineName, int majorVersion, int minorVersion, int order);

        protected abstract void SetLibraryOrderInContent(string contentId, string machineName, int majorVersion, int minorVersion, int order);

        protected abstract IEnumerable<H5PLibraryForContentItemDto> GetLibrariesForContentItems(IEnumerable<string> contentIds);

        protected abstract void SaveChanges();
    }
}

