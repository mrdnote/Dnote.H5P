using System.Collections.Generic;

namespace Dnote.H5P.Dto
{
    public class H5PContentItemFileDto
    {
        public string ContentId { get; set; } = null!;

        public string Title { get; set; } = null!;

        public string Content { get; set; } = null!;

        public class Library
        {
            public string MachineName { get; set; } = null!;

            public int MajorVersion { get; set; }

            public int MinorVersion { get; set; }

            public IEnumerable<string> JsFiles { get; set; } = null!;

            public IEnumerable<string> CssFiles { get; set; } = null!;

            public int Order { get; set; }
        }

        public List<Library> Libraries { get; set; } = new List<Library>();

        public Library MainLibrary { get; set; } = null!;
    }
}
