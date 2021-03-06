﻿using System;
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
        private readonly string? _pathPrefix = null;

        #region Public methods

        public H5PMetaDataAgent(string? pathPrefix)
        {
            _pathPrefix = pathPrefix;
        }

        public void LoadContent(string contentId)
        {
            var contentIds = new string[] { contentId };
            InnerLoadContent(contentIds);
        }

        public void LoadContent(IEnumerable<string> contentIds)
        {
            InnerLoadContent(contentIds);
        }

        public void StoreContentItem(H5PJsonDto h5pJson, string contentId, string content)
        {
            InnerStoreContentItem(h5pJson, contentId, content);

            var touchedLibraries = new List<(string, int, int)>();

            if (h5pJson.PreloadedDependencies != null)
            {
                SaveContentDependencies(h5pJson.PreloadedDependencies, contentId, touchedLibraries, h5pJson.MainLibrary);
            }
        }

        public IEnumerable<H5PContentItemDto> GetContentItems()
        {
            return InnerGetContentItems();
        }

        public H5PContentItemDto GetContentItem(string contentId)
        {
            return InnerGetContentItems().FirstOrDefault(i => i.ContentId == contentId);
        }

        #endregion

        /// <summary>
        /// Saves all the loaded and stored content items again.
        /// </summary>
        public abstract void SaveContent();

        private void SaveContentDependencies(IEnumerable<H5PJsonDto.Dependency> dependencies, string contentId, List<(string, int, int)> touchedLibraries, string mainLibrary)
        {
            foreach (var dependency in dependencies)
            {
                ProcessLibrary(dependency.Library, contentId, touchedLibraries, mainLibrary);
            }
        }

        public string GetPrefix()
        {
            var result = GetFileSystemPrefix() + _pathPrefix;

            return result.TrimEnd('/');
        }

        private void ProcessLibrary(H5PJsonDto.Library library, string contentId, List<(string, int, int)> touchedLibraries, string mainLibrary)
        {
            var dependencies = library.PreloadedDependencies;

            if (dependencies != null)
            {
                foreach (var dependency in dependencies)
                {
                    ProcessLibrary(dependency.Library, contentId, touchedLibraries, mainLibrary);
                }
            }

            SaveLibrary(library.Title, library.MachineName, library.MajorVersion, library.MinorVersion, library.PatchVersion, library.CoreApi?.MajorVersion, library.CoreApi?.MinorVersion, 
                library.Author, library.PreloadedJs?.Select(f => f.Path), library.PreloadedCss?.Select(f => f.Path));

            if (!touchedLibraries.Any(l => l.Item1 == library.MachineName && l.Item2 == library.MajorVersion && l.Item3 == library.MinorVersion))
            {
                var isMainLibrary = library.MachineName == mainLibrary;
                UpdateLibraryInContent(contentId, library.MachineName, library.MajorVersion, library.MinorVersion, library.PreloadedJs?.Select(f => f.Path), library.PreloadedCss?.Select(f => f.Path), 
                    touchedLibraries.Count, isMainLibrary);
                touchedLibraries.Add((library.MachineName, library.MajorVersion, library.MinorVersion));
            }
        }

        public string GetIncludeFilesCsv(string contentId, FileTypes fileType)
        {
            return string.Join(",", GetIncludeFiles(contentId, fileType));
        }

        public string GetLibrary(string contentId)
        {
            var library = InnerGetContentItem(contentId).MainLibrary;
            return $"{library.MachineName} {library.MajorVersion}.{library.MinorVersion}";
        }

        protected abstract H5PContentItemDto InnerGetContentItem(string contentId);

        public string GetTitle(string contentId)
        {
            return InnerGetContentItem(contentId).Title;
        }

        public IEnumerable<string> GetIncludeFiles(string contentId, FileTypes fileType)
        {
            var libraries = GetLibrariesForContentItem(contentId);

            var pathPrefix = _pathPrefix;

            if (pathPrefix != null && pathPrefix != "")
            {
                pathPrefix = pathPrefix.TrimEnd('/') + "/";
            }

            var fileSystemPrefix = GetFileSystemPrefix();

            if (fileType == FileTypes.Js)
            {
                return libraries.SelectMany(l => l.JsFiles.Select(f => $"{fileSystemPrefix}{pathPrefix}{l.MachineName}-{l.MajorVersion}.{l.MinorVersion}/{f}")).Distinct();
            }
            else if (fileType == FileTypes.Css)
            {
                return libraries.SelectMany(l => l.CssFiles.Select(f => $"{fileSystemPrefix}{pathPrefix}{l.MachineName}-{l.MajorVersion}.{l.MinorVersion}/{f}")).Distinct();
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(fileType));
            }
        }

        public string GetJsonContent(string contentId)
        {
            return InnerGetContentItem(contentId).Content;
        }

        protected abstract void InnerLoadContent(IEnumerable<string> contentIds);

        protected abstract void InnerStoreContentItem(H5PJsonDto h5pJson, string contentId, string content);

        protected abstract IEnumerable<H5PContentItemDto> InnerGetContentItems();

        protected abstract void SaveLibrary(string title, string machineName, int majorVersion, int minorVersion, int patchVersion, int? coreApiMajorVersion, int? coreApiMinorVersion, 
            string author, IEnumerable<string>? jsFiles, IEnumerable<string>? cssFiles);

        /// <summary>
        /// Implement to update the library to the content item. If the library does not yet exist in the content item, add it. If it exists but has a different version, replace it. If it exists
        /// and has the same version, do nothing.
        /// </summary>
        protected abstract void UpdateLibraryInContent(string contentId, string machineName, int majorVersion, int minorVersion, IEnumerable<string>? jsFiles, IEnumerable<string>? cssFiles, int order,
            bool isMainLibrary);

        protected abstract IEnumerable<H5PLibraryForContentItemDto> GetLibrariesForContentItem(string contentId);

        protected abstract string? GetFileSystemPrefix();
    }
}

