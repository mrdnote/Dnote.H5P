﻿using System.IO;
using System.Threading.Tasks;

namespace Dnote.H5P
{
    /// <summary>
    /// Base class for the agent that stores H5P type content (css, js, fonts etc) in the file system or cloud, from where it can be inserted into the html.
    /// </summary>
    public abstract class H5PStorageAgent
    {
        public abstract Task StoreFileAsync(Stream stream, string fileName);
    }
}
