#nullable enable
using System.Text;
using System.Web.Mvc;
using Dnote.H5P.Enums;

namespace Dnote.H5P.NetFW.UI
{
    public static class HtmlHelperExtensions
    {
        public static MvcHtmlString H5PCssIncludes(this HtmlHelper helper, H5PMetaDataAgent metaDataAgent, params string[] contentIds)
        {
            var cssFiles = metaDataAgent.GetIncludeFilesForContentItems(contentIds, FileTypes.Css);

            var sb = new StringBuilder();

            foreach (var cssFile in cssFiles)
            {
                sb.AppendLine($"<link href=\"{cssFile}\" rel=\"stylesheet\"/>");
            }

            return new MvcHtmlString(sb.ToString());
        }

        public static MvcHtmlString H5PJsIncludes(this HtmlHelper helper, H5PMetaDataAgent metaDataAgent, params string[] contentIds)
        {
            var jsFiles = metaDataAgent.GetIncludeFilesForContentItems(contentIds, FileTypes.Js);

            var sb = new StringBuilder();

            foreach (var ksFile in jsFiles)
            {
                sb.AppendLine($"<script src=\"{ksFile}\"></script>");
            }

            return new MvcHtmlString(sb.ToString());
        }
    }
}
