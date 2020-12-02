using Dnote.H5P.Dto;
using System.Collections.Generic;
using System.Linq;

namespace Dnote.H5P
{
    /// <summary>
    /// Base class for the agent that stores the H5P type meta data. This meta data is used to render the content and all its dependencies later on.
    /// </summary>
    public abstract class H5PMetaDataAgent
    {
        public void StoreContent(H5PJsonDto h5pJson, string contentId)
        {
            SaveContent(contentId, h5pJson.Title, h5pJson.Language, h5pJson.EmbedTypes, h5pJson.Licence, h5pJson.DefaultLanguage);

            if (h5pJson.PreloadedDependencies != null)
            {
                SaveDependencies(h5pJson.PreloadedDependencies);
            }
        }

        private void SaveDependencies(IEnumerable<H5PJsonDto.Dependency> dependencies)
        {
            foreach (var dependency in dependencies)
            {
                SaveContentDependency(dependency.MachineName, dependency.MajorVersion, dependency.MinorVersion, dependency.Library.PatchVersion, dependency.Library.Title, dependency.Library.Description,
                    dependency.Library.Runnable, dependency.Library.Author, dependency.Library.Licence, dependency.Library.PreloadedJs?.Select(lf => lf.Path),
                    dependency.Library.PreloadedCss?.Select(lf => lf.Path));

                if (dependency.Library.PreloadedDependencies != null)
                {
                    SaveDependencies(dependency.Library.PreloadedDependencies);
                }
            }
        }

        protected abstract void SaveContent(string contentId, string title, string language, string[] embedTypes, string licence, string defaultLanguage);

        protected abstract void SaveContentDependency(string machineName, int majorVersion, int minorVersion, int patchVersion, string title, string description, int runnable, string author, string licence, IEnumerable<string>? enumerable1, IEnumerable<string>? enumerable2);
    }
}
