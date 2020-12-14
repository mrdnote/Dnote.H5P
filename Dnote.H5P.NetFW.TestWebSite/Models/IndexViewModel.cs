using System.Collections.Generic;
using System.Web.Mvc;
using Dnote.H5P.NetFW.TestWebSite.Enums;

namespace Dnote.H5P.NetFW.TestWebSite.Models
{
    public class IndexViewModel
    {
        public IEnumerable<string> ContentItems { get; set; } = null!;

        public StorageEnum Storage { get; set; }

        public List<SelectListItem> Storages { get; internal set; }
    }
}