using System.Collections.Generic;

namespace Dnote.H5P.Dto
{
    public class H5PJsonDto
    {
        public string Title { get; set; } = null!;

        public string Language { get; set; } = null!;

        public string MainLibrary { get; set; } = null!;

        public string[] EmbedTypes { get; set; } = null!;

        public string License { get; set; } = null!;

        public string DefaultLanguage { get; set; } = null!;

        public class LibraryFile
        {
            public string Path { get; set; } = null!;
        }

        public class Version
        {
            public int MajorVersion { get; set; }
            
            public int MinorVersion { get; set; }
        }

        public class Library
        {
            public string MachineName { get; set; } = null!;

            public int MajorVersion { get; set; }

            public int MinorVersion { get; set; }

            public string Title { get; set; } = null!;
            
            public string Description { get; set; } = null!;

            public int PatchVersion { get; set; }

            public int Runnable { get; set; }

            public string Author { get; set; } = null!;

            public string License { get; set; } = null!;

            public Version? CoreApi { get; set; } = null!;

            public IEnumerable<LibraryFile> PreloadedJs { get; set; } = null!;

            public IEnumerable<LibraryFile> PreloadedCss { get; set; } = null!;

            public IEnumerable<Dependency>? PreloadedDependencies { get; set; }
        }

        public class Dependency
        {
            public string MachineName { get; set; } = null!;

            public int MajorVersion { get; set; }

            public int MinorVersion { get; set; }

            public Library Library { get; set; } = null!;
        }

        public IEnumerable<Dependency>? PreloadedDependencies { get; set; }
    }
}
