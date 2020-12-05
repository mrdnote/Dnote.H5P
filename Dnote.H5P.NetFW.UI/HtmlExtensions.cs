#nullable enable
using System.Text;
using System.Web.Mvc;
using Dnote.H5P.Enums;

namespace Dnote.H5P.NetFW.UI
{
    public static class HtmlHelperExtensions
    {
        public static MvcHtmlString H5PCssIncludes(this HtmlHelper helper, H5PMetaDataAgent metaDataAgent)
        {
            _ = helper;

            var cssFiles = metaDataAgent.GetIncludeFilesForContentItems(FileTypes.Css);

            var sb = new StringBuilder();

            foreach (var cssFile in cssFiles)
            {
                sb.AppendLine($"<link href=\"{cssFile}\" rel=\"stylesheet\"/>");
            }

            return new MvcHtmlString(sb.ToString());
        }

        public static MvcHtmlString H5PJsIncludes(this HtmlHelper helper, H5PMetaDataAgent metaDataAgent)
        {
            _ = helper;

            var jsFiles = metaDataAgent.GetIncludeFilesForContentItems(FileTypes.Js);

            var sb = new StringBuilder();

            foreach (var ksFile in jsFiles)
            {
                sb.AppendLine($"<script src=\"{ksFile}\"></script>");
            }

            return new MvcHtmlString(sb.ToString());
        }

        public static MvcHtmlString H5PScript(this HtmlHelper helper, H5PMetaDataAgent metaDataAgent, UrlHelper urlHelper)
        {
            var sb = new StringBuilder();

            var items = metaDataAgent.GetContentItems();

            sb.AppendLine("<script>");

            var mainScript = $@"
                window.H5PIntegration = {{
                    'baseUrl': 'http://www.mysite.com',     // No trailing slash
                    'url': '{urlHelper.Content("~/Content/")}',   // Relative to web root
                    'postUserStatistics': true,             // Only if user is logged in
                    'ajaxPath': '/path/to/h5p-ajax',        // Only used by older Content Types
                    'ajax': {{
                        // Where to post user results
                        'setFinished': '/interactive-contents/123/results/new',
                        // Words beginning with : are placeholders
                        'contentUserData': '/interactive-contents/:contentId/user-data?data_type=:dataType&subContentId=:subContentId'
                    }},
                    'saveFreq': 5, // How often current content state should be saved. false to disable.
                    'user': {{ // Only if logged in !
                        'name': 'User Name',
                        'mail': 'user@mysite.com'
                    }},
                    'siteUrl': 'http://www.mysite.com', // Only if NOT logged in!
                    'l10n': {{ // Text string translations
                        'H5P': {{
                            'fullscreen': 'Fullscreen',
                            'disableFullscreen': 'Disable fullscreen',
                            'download': 'Download',
                            'copyrights': 'Rights of use',
                            'embed': 'Embed',
                            'size': 'Size',
                            'showAdvanced': 'Show advanced',
                            'hideAdvanced': 'Hide advanced',
                            'advancedHelp': 'Include this script on your website if you want dynamic sizing of the embedded content:',
                            'copyrightInformation': 'Rights of use',
                            'close': 'Close',
                            'title': 'Title',
                            'author': 'Author',
                            'year': 'Year',
                            'source': 'Source',
                            'license': 'License',
                            'thumbnail': 'Thumbnail',
                            'noCopyrights': 'No copyright information available for this content.',
                            'downloadDescription': 'Download this content as a H5P file.',
                            'copyrightsDescription': 'View copyright information for this content.',
                            'embedDescription': 'View the embed code for this content.',
                            'h5pDescription': 'Visit H5P.org to check out more cool content.',
                            'contentChanged': 'This content has changed since you last used it.',
                            'startingOver': 'You\'ll be starting over.',
                            'by': 'by',
                            'reuse': 'Reuse',
                            'showMore': 'Show more',
                            'showLess': 'Show less',
                            'subLevel': 'Sublevel'
                        }}
                    }},
                    'contents': []
                }};
            ";

            sb.AppendLine(mainScript);

            foreach (var item in items)
            {
                var script = $@"
                    window.H5PIntegration.contents['cid-{item.ContentId}'] = {{
                        'library': '{item.MainLibrary.MachineName} {item.MainLibrary.MajorVersion}.{item.MainLibrary.MinorVersion}', // Library name + major version.minor version
                        'jsonContent': '{item.Content}',
                        'fullScreen': false, // No fullscreen support
                        'exportUrl': '/path/to/download.h5p',
                        'mainId': 1234,
                        'url': 'https://mysite.com/h5p/1234',
                        'title': '{item.Title}',
                        'contentUserData': {{
                            0: {{ // Sub ID (0 = main content/no id)
                                'state': false // => FALSE // Data ID
                            }}
                        }},
                        'displayOptions': {{
                            'frame': true, // Show frame and buttons below H5P
                            'export': true, // Display download button
                            'embed': true, // Display embed button
                            'copyright': true, // Display copyright button
                            'icon': true // Display H5P icon
                        }}
                    }};

                    $(document).ready(function ()
                    {{
                        H5P.init();
                    }});
                ";

                sb.AppendLine(script);
            }

            sb.AppendLine("</script>");

            return new MvcHtmlString(sb.ToString());
        }
    }
}
