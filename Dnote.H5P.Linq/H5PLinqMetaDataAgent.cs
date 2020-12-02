using System.Collections.Generic;

namespace Dnote.H5P.Linq
{
    public class H5PLinqMetaDataAgent : H5PMetaDataAgent
    {
        protected override void SaveContent(string contentId, string title, string language, string[] embedTypes, string licence, string defaultLanguage)
        {
            throw new System.NotImplementedException();
        }

        protected override void SaveContentDependency(string machineName, int majorVersion, int minorVersion, int patchVersion, string title, string description, int runnable, string author, string licence, IEnumerable<string> enumerable1, IEnumerable<string> enumerable2)
        {
            throw new System.NotImplementedException();
        }
    }
}
