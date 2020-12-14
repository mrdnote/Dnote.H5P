#nullable enable
using Dnote.H5P.NetFW.TestWebSite.Enums;

namespace Dnote.H5P.NetFW.TestWebSite.Models
{
    public class ExerciseViewModel
    {
        public string Id { get; set; } = null!;

        public string Title { get; set; } = null!;

        public string? State { get; set; }

        public bool Visible { get; set; } = true;

        public StorageEnum Storage { get; set; }

        public H5PMetaDataAgent H5PMetaDataAgent { get; set; } = null!;
    }
}