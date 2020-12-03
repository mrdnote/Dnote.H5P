using System.Collections.Generic;

namespace Dnote.H5P.Dto
{
    public class H5PLibraryForContentItemDto
    {
        public string MachineName { get; set; } = null!;

        public int MinorVersion { get; set; }

        public int MajorVersion { get; set; }

        public IEnumerable<string> JsFiles { get; set; } = null!;

        public IEnumerable<string> CssFiles { get; set; } = null!;
    }
}
