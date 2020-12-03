#nullable enable
namespace Dnote.H5P.NetFW.TestWebSite.Models
{
    public class ExerciseViewModel
    {
        public string Id { get; set; } = null!;

        public string Title { get; set; } = null!;

        public H5PMetaDataAgent H5PMetaDataAgent { get; set; } = null!;
    }
}