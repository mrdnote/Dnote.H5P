namespace Dnote.H5P.NetFW.Api.Model
{
    public class SetFinishedBodyModel
    {
        public string ContentId { get; set; }
        public double Score { get; set; }
        public double MaxScore { get; set; }
        public int Opened { get; set; }
        public int Finished { get; set; }
        public int? Time { get; set; }
    }
}
