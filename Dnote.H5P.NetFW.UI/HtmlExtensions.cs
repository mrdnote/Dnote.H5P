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

        public static MvcHtmlString H5PMainScript(this HtmlHelper helper, H5PMetaDataAgent metaDataAgent, string apiUrlPrefix, int? saveFreq)
        {
            _ = helper;

            var sb = new StringBuilder();

            sb.AppendLine("<script>");

            var mainScript = $@"
                if (!window.H5PCompleted)
                {{
                    window.H5PCompleted = {{}};
                }}
                if (!window.H5PIntegration)
                {{
                window.H5PIntegration = {{
                    'baseUrl': 'http://www.mysite.com',     // No trailing slash
                    'url': '{metaDataAgent.GetPrefix()}',   // Relative to web root
                    'postUserStatistics': {(apiUrlPrefix != null ? "true" : "false")},             // Only if user is logged in
                    'ajaxPath': '/{apiUrlPrefix}', // Only used by older Content Types
                    'ajax': {{
                        // Where to post user results
                        'setFinished': {(apiUrlPrefix != null ? "'/" + apiUrlPrefix.TrimEnd('/') + "/SetFinished'" : "null")},
                        // Words beginning with : are placeholders
                        'contentUserData': {(apiUrlPrefix != null ? "'/" + apiUrlPrefix.TrimEnd('/') + "/UserData/:contentId?data_type=:dataType&subContentId=:subContentId'" : "null")},
                    }},
                    'saveFreq': {saveFreq?.ToString() ?? "false"}, // How often current content state should be saved. false to disable.
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
            }}
            ";

            sb.AppendLine(mainScript);

            sb.AppendLine("</script>");

            return new MvcHtmlString(sb.ToString());
        }

        public static MvcHtmlString H5PItemsScript(this HtmlHelper helper, H5PMetaDataAgent metaDataAgent)
        {
            _ = helper;

            var sb = new StringBuilder();

            var items = metaDataAgent.GetContentItems();

            sb.AppendLine("<script>");

            foreach (var item in items)
            {
                // If userContent is null, the content item has not yet been submitted. If userContent is an empty string, the content item has been submitted, but the content type does not support
                // state. If userContent is filled, it is filled with the answers the user previously submitted.
                if (item.Render)
                {
                    var script = $@"
                        const jsonContent = '{item.Content.Replace("\\n", "\\\\n").Replace("\n", "")}';
                        window.H5PCompleted['{item.ContentId}'] = {(item.UserContent != null ? "true" : "false")};
                        window.H5PIntegration.contents['cid-{item.ContentId}'] = {{
                            'library': '{item.MainLibrary.MachineName} {item.MainLibrary.MajorVersion}.{item.MainLibrary.MinorVersion}', // Library name + major version.minor version
                            'jsonContent': jsonContent,
                            'fullScreen': false, // No fullscreen support
                            'exportUrl': '/path/to/download.h5p',
                            'mainId': 1234,
                            'url': 'https://mysite.com/h5p/1234',
                            'title': '{item.Title}',
                            'contentUserData': {{
                                0: {{ // Sub ID (0 = main content/no id)
                                    'state': '{item.UserContent}'
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
                    ";

                    sb.AppendLine(script);
                }
            }

            sb.AppendLine("</script>");

            return new MvcHtmlString(sb.ToString());
        }
    }
}
