#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Dnote.H5P.Dto;
using Dnote.H5P.Enums;
using Dnote.H5P.NetFW.Linq2Sql.Interfaces;

namespace Dnote.H5P.NetFW.Linq2Sql
{
    public class H5PLinqMetaDataAgent : H5PMetaDataAgent
    {
        private readonly IH5PDataContext _context;

        public H5PLinqMetaDataAgent(IH5PDataContext context, string pathPrefix) 
            : base(pathPrefix)
        {
            _context = context;
        }

        protected override void SaveContent(string contentId, string title, string language, string license, string defaultLanguage, IEnumerable<string> embedTypes)
        {
            var content = _context.Repository<H5P_ContentItem>().FirstOrDefault(c => c.ContentId == contentId);
            
            if (content == null)
            {
                content = new H5P_ContentItem
                {
                    ContentId = contentId
                };
                _context.Insert(content);
            }

            content.Title = title;
            content.Language = language;
            content.License = license;
            content.DefaultLanguage = defaultLanguage;

            var existingEmbedTypes = content.H5P_ContentItem_EmbedTypes.Select(t => t.Value);
            var embedTypesToDelete = content.H5P_ContentItem_EmbedTypes.Where(t => !embedTypes.Contains(t.Value));
            var embedTypesToAdd = embedTypes.Except(existingEmbedTypes);
            foreach (var embedTypeToAdd in embedTypesToAdd)
            {
                content.H5P_ContentItem_EmbedTypes.Add(new H5P_ContentItem_EmbedType
                {
                    H5P_ContentItem = content,
                    Value = embedTypeToAdd
                });
            }
            _context.DeleteAll(embedTypesToDelete);
        }

        protected override void SaveLibrary(string title, string machineName, int majorVersion, int minorVersion, int patchVersion, int? coreApiMajorVersion, int? coreApiMinorVersion, 
            string author, IEnumerable<string>? jsFiles, IEnumerable<string>? cssFiles)
        {
            var now = DateTime.UtcNow;

            var library = _context.Repository<H5P_Library>().FirstOrDefault(l => l.MachineName == machineName && l.MajorVersion == majorVersion && l.MinorVersion == minorVersion);
            if (library == null)
            {
                library = new H5P_Library
                {
                    MachineName = machineName,
                    MajorVersion = majorVersion,
                    MinorVersion = minorVersion,
                    AddedOn = now
                };

                _context.Insert(library);
            }

            if (library.Id == 0 || library.PatchVersion < patchVersion)
            {
                library.Title = title;
                library.PatchVersion = patchVersion;
                library.CoreApiMajorVersion = coreApiMajorVersion;
                library.CoreApiMinorVersion = coreApiMinorVersion;
                library.Author = author;
                library.UpdatedOn = now; 

                SyncLibraryFiles(library, jsFiles, FileTypes.Js);
                SyncLibraryFiles(library, cssFiles, FileTypes.Css);
            }
        }

        private void SyncLibraryFiles(H5P_Library library, IEnumerable<string>? paths, FileTypes fileType)
        {
            var filesOfType = library.H5P_Library_Files.Where(f => f.Type == (int)fileType).ToList();

            var pathsList = paths != null ? paths.ToList() : new List<string>();

            for (var i = 0; i < pathsList.Count; i++)
            {
                var path = pathsList[i];
                if (filesOfType.Count() <= i)
                {
                    filesOfType.Add(new H5P_Library_File 
                    {
                        H5P_Library = library,
                        Type = (int)fileType
                    });
                }
                filesOfType[i].Path = path;
                filesOfType[i].Order = i;
            }

            var filesToDelete = filesOfType.Skip(pathsList.Count);
            _context.DeleteAll(filesToDelete);
        }

        protected override bool LibraryExistsInContent(string contentId, string machineName, int majorVersion, int minorVersion)
        {
            return _context.Repository<H5P_ContentItem_Library>().Any(cl => cl.H5P_ContentItem.ContentId == contentId && cl.H5P_Library.MachineName == machineName && cl.H5P_Library.MajorVersion == majorVersion && cl.H5P_Library.MinorVersion == minorVersion);
        }

        protected override void AddLibraryToContent(string contentId, string machineName, int majorVersion, int minorVersion, int order)
        {
            if (LibraryExistsInContent(contentId, machineName, majorVersion, minorVersion))
            {
                throw new Exception("Library already exists in content item!");
            }

            var library = _context.Repository<H5P_Library>().First(l => l.MachineName == machineName && l.MajorVersion == majorVersion && l.MinorVersion == minorVersion);
            var content = _context.Repository<H5P_ContentItem>().First(c => c.ContentId == contentId);

            var contentLibrary = new H5P_ContentItem_Library
            {
                H5P_ContentItem = content,
                H5P_Library = library,
                Order = order
            };

            _context.Insert(contentLibrary);
        }

        protected override void SetLibraryOrderInContent(string contentId, string machineName, int majorVersion, int minorVersion, int order)
        {
            var contentLibrary = _context.Repository<H5P_ContentItem_Library>().First(cl => cl.H5P_ContentItem.ContentId == contentId && cl.H5P_Library.MachineName == machineName && cl.H5P_Library.MajorVersion == majorVersion && cl.H5P_Library.MinorVersion == minorVersion);

            contentLibrary.Order = order;
        }

        protected override void SaveChanges()
        {
            _context.SubmitChanges();
        }

        protected override IEnumerable<H5PLibraryForContentItemDto> GetLibrariesForContentItems(IEnumerable<string> contentIds)
        {
            var contentLibraries = _context.Repository<H5P_ContentItem_Library>().Where(l => contentIds.Contains(l.H5P_ContentItem.ContentId)).Distinct();
            var jsFiles = _context.Repository<H5P_Library_File>().Where(f => f.Type == (int)FileTypes.Js);
            var cssFiles = _context.Repository<H5P_Library_File>().Where(f => f.Type == (int)FileTypes.Css);
            var groups = contentLibraries
                .GroupJoin(jsFiles, cl => cl.LibraryId, js => js.LibraryId, (k, v) => new { ContentLibrary = k, Library = k.H5P_Library, JsFiles = v.OrderBy(f => f.Order) })
                .GroupJoin(cssFiles, g1 => g1.ContentLibrary.LibraryId, css => css.LibraryId, (k, v) => new { k.ContentLibrary, k.Library, k.JsFiles, CssFiles = v.OrderBy(f => f.Order) })
                .OrderBy(g2 => g2.ContentLibrary.Order)
                ;

            return groups.Select(cl => new H5PLibraryForContentItemDto
            {
                MachineName = cl.Library.MachineName,
                MajorVersion = cl.Library.MajorVersion,
                MinorVersion = cl.Library.MinorVersion,
                JsFiles = cl.JsFiles.Select(f => f.Path),
                CssFiles = cl.CssFiles.Select(f => f.Path),
            });
        }
    }
}
