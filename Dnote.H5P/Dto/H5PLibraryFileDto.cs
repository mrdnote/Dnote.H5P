using System.Collections.Generic;

namespace Dnote.H5P.Dto
{
    internal class H5PLibraryFileDto
    {
        public IEnumerable<string>? JsFiles { get; set; }

        public IEnumerable<string>? CssFiles { get; set; }
    }
}
