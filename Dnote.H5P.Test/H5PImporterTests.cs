using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace Dnote.H5P.Test
{
    [TestClass]
    public class H5PImporterTests
    {
        [TestMethod]
        public void ImportTest()
        {
            var fileName = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "TestFiles\\test-mc-1291177782275013467.h5p");
            var targetPath = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "ImportFiles");
            if (Directory.Exists(targetPath))
            {
                Directory.Delete(targetPath, true);
            }
            Directory.CreateDirectory(targetPath);
            var storageAgent = new H5PFileStorageAgent(targetPath);
            var databaseAgent = new H5PInMemoryMetaDataAgent();
            var importer = new H5PImporter(storageAgent, databaseAgent);
            importer.Import(fileName);

            Assert.IsTrue(File.Exists(Path.Combine(targetPath, "Drop-1.0", "css", "drop-theme-arrows-bounce.min.css")));
            Assert.IsTrue(File.Exists(Path.Combine(targetPath, "Drop-1.0", "js", "drop.min.js")));
            Assert.IsTrue(File.Exists(Path.Combine(targetPath, "EmbeddedJS-1.0", "js", "ejs_production.js")));
            Assert.IsTrue(File.Exists(Path.Combine(targetPath, "EmbeddedJS-1.0", "js", "ejs_viewhelpers.js")));
            Assert.IsTrue(File.Exists(Path.Combine(targetPath, "FontAwesome-4.5", "h5p-font-awesome.min.css")));
            Assert.IsTrue(File.Exists(Path.Combine(targetPath, "FontAwesome-4.5", "fontawesome-webfont.eot")));
        }
    }
}