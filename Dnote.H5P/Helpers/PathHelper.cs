using Dnote.H5P.Enums;
using MimeTypes;
using System.IO;

namespace Dnote.H5P.Helpers
{
    public abstract class PathHelper
    {
        public static FileType GetFileType(string filename)
        {
            if (filename == null)
            {
                return FileType.None;
            }

            var ext = Path.GetExtension(filename).ToLower();
            if (ext == ".mp4" || ext == ".webm")
            {
                return FileType.Video;
            }

            if (ext == ".ttf" || ext == ".woff2")
            {
                return FileType.Font;
            }

            var mime = MimeTypeMap.GetMimeType(ext);
            if (mime.StartsWith("image/"))
            {
                return FileType.Image;
            }

            if (mime.StartsWith("video/"))
            {
                return FileType.Video;
            }

            if (mime.StartsWith("audio/"))
            {
                return FileType.Audio;
            }

            if (mime.EndsWith("/css"))
            {
                return FileType.StyleSheet;
            }

            if (mime.EndsWith("/javascript"))
            {
                return FileType.Javascript;
            }

            if (mime.EndsWith("/font-woff") || mime.EndsWith("/vnd.ms-fontobject"))
            {
                return FileType.Font;
            }

            return FileType.File;
        }

        public static bool IsCacheableFileType(string filename)
        {
            var type = GetFileType(filename);

            return type == FileType.Image || type == FileType.Audio || type == FileType.Video || type == FileType.StyleSheet || type == FileType.Javascript || type == FileType.Font;
        }
    }
}
