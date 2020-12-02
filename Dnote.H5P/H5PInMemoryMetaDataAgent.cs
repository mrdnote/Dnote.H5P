using System.Collections.Generic;
using System.Linq;

namespace Dnote.H5P
{
    /// <inheritdoc/>
    public class H5PInMemoryMetaDataAgent : H5PMetaDataAgent
    {
        private readonly List<Content> _contentItems;

        public class Content
        {
            public string Id { get; set; } = null!;
        }

        public H5PInMemoryMetaDataAgent()
        {
            _contentItems = new List<Content>();
        }

        protected override void SaveContent(string contentId, string title, string language, string[] embedTypes, string licence, string defaultLanguage)
        {
            var content = _contentItems.FirstOrDefault(i => i.Id == contentId);

            if (content == null)
            {
                content = new Content
                {
                    Id = contentId
                };

                _contentItems.Add(content);
            }
        }

        protected override void SaveContentDependency(string machineName, int majorVersion, int minorVersion, int patchVersion, string title, string description, int runnable, string author, string licence, IEnumerable<string>? enumerable1, IEnumerable<string>? enumerable2)
        {
        }
    }
}
