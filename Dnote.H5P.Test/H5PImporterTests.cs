#nullable enable
using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dnote.H5P;
using Dnote.H5P.Enums;
using System.Threading.Tasks;

namespace Dnote.H5P.Test
{
    [TestClass]
    public class H5PImporterTests
    {
        [TestInitialize()]
        public void Initialize() 
        {
        }

        [TestMethod]
        public void ImportTest()
        {
            var fileName = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "TestFiles\\test-mc-1291177782275013467.h5p");
            var targetPath = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "ImportFiles");
            //var metaDataPath = Path.Combine(targetPath, "MetaData");
            if (Directory.Exists(targetPath))
            {
                Directory.Delete(targetPath, true);
                while (Directory.Exists(targetPath))
                {
                    System.Threading.Thread.Sleep(10);
                }
            }
            Directory.CreateDirectory(targetPath);

            var storageAgent = new H5PFileStorageAgent(targetPath);
            var metaDataAgent = new H5PPhysicalFileMetaDataAgent("ImportFiles", targetPath);
            var importer = new H5PImporter(storageAgent, metaDataAgent);

            importer.Import(fileName);

            Assert.IsTrue(File.Exists(Path.Combine(targetPath, "EmbeddedJS-1.0", "js", "ejs_production.js")));
            Assert.IsTrue(File.Exists(Path.Combine(targetPath, "EmbeddedJS-1.0", "js", "ejs_viewhelpers.js")));
            Assert.IsTrue(File.Exists(Path.Combine(targetPath, "FontAwesome-4.5", "h5p-font-awesome.min.css")));
            Assert.IsTrue(File.Exists(Path.Combine(targetPath, "Tether-1.0", "styles", "tether.min.css")));

            Assert.IsTrue(File.Exists(Path.Combine(targetPath, "FontAwesome-4.5", "fontawesome-webfont.eot")));

            var jsIncludeFiles = metaDataAgent.GetIncludeFilesForContentItems(FileTypes.Js);

            Assert.AreEqual(20, jsIncludeFiles.Count());
            Assert.AreEqual("ImportFiles/EmbeddedJS-1.0/js/ejs_production.js", jsIncludeFiles.First());
            Assert.AreEqual("ImportFiles/EmbeddedJS-1.0/js/ejs_viewhelpers.js", jsIncludeFiles.Skip(1).First());

            var cssIncludeFiles = metaDataAgent.GetIncludeFilesForContentItems(FileTypes.Css);

            Assert.AreEqual(18, cssIncludeFiles.Count());
            Assert.AreEqual("ImportFiles/FontAwesome-4.5/h5p-font-awesome.min.css", cssIncludeFiles.First());
            Assert.AreEqual("ImportFiles/Tether-1.0/styles/tether.min.css", cssIncludeFiles.Skip(1).First());
        }
    }
}