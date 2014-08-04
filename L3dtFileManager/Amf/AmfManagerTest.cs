using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace L3dtFileManager.Amf
{
    using NUnit.Framework;

    [TestFixture]
    class AmfManagerTest
    {
        //change this to your path before running the test
        private String pathToTestFile = "C:\\Git\\Repos\\L3DT_Filemanager\\L3dtFileManager";

        [Test]
        public void AmfUnCompressedReadTest()
        {
            AmfManager manager = new AmfManager();
            AmfFile file = manager.loadFile(pathToTestFile + "\\testfile\\test_AM.amf", FileFormat.UNCOMPRESSED);

            Assert.AreEqual(1024, file.width);
            Assert.AreEqual(1024, file.height);
            Assert.AreEqual(file.width * file.height, file.pixels.Count());
        }

        [Test]
        public void AmfCompressedReadTest()
        {
            AmfManager manager = new AmfManager();
            AmfFile file = manager.loadFile(pathToTestFile + "\\testfile\\test_AM.amf.gz", FileFormat.COMPRESSED);

            Assert.AreEqual(1024, file.width);
            Assert.AreEqual(1024, file.height);
            Assert.AreEqual(file.width * file.height, file.pixels.Count());
        }

        [Test]
        public void AmfGetPixelAtTest()
        {
            AmfManager manager = new AmfManager();
            AmfFile file = manager.loadFile(pathToTestFile + "\\testfile\\test_AM.amf.gz", FileFormat.COMPRESSED);

            AmfPixelInfo value = file.getPixelAt(300, 400);
            Assert.AreEqual(3, value.landTypeId);
            Assert.AreEqual(1, value.climateId);
        }

        [Test]
        [ExpectedException]
        public void AmfBadMarkerReadTest()
        {
            AmfManager manager = new AmfManager();
            AmfFile file = manager.loadFile(pathToTestFile + "\\testfile\\test_AM_bad_marker.amf", FileFormat.UNCOMPRESSED);
        }

        [Test]
        [ExpectedException]
        public void AmfBadFileTypeReadTest()
        {
            AmfManager manager = new AmfManager();
            AmfFile file = manager.loadFile(pathToTestFile + "\\testfile\\test_AM_bad_file_type.amf", FileFormat.UNCOMPRESSED);
        }

        [Test]
        public void AmfUnCompressedWriteTest()
        {
            string copyTestFile = pathToTestFile + "\\testfile\\filecopy.amf";
            AmfManager manager = new AmfManager();
            AmfFile file = manager.loadFile(pathToTestFile + "\\testfile\\test_AM.amf", FileFormat.UNCOMPRESSED);
            manager.saveFile(copyTestFile, FileFormat.UNCOMPRESSED, file);
            AmfFile filecopy = manager.loadFile(copyTestFile, FileFormat.UNCOMPRESSED);
            
            Assert.AreEqual(file.width, filecopy.width);
            Assert.AreEqual(file.width, filecopy.width);
            Assert.AreEqual(file.pixels.Count(), filecopy.pixels.Count());

            File.Delete(copyTestFile);
        }

        [Test]
        public void AmfCompressedWriteTest()
        {
            string copyTestFile = pathToTestFile + "\\testfile\\filecopy.amf.gz";
            AmfManager manager = new AmfManager();
            AmfFile file = manager.loadFile(pathToTestFile + "\\testfile\\test_AM.amf.gz", FileFormat.COMPRESSED);
            manager.saveFile(copyTestFile, FileFormat.COMPRESSED, file);
            AmfFile filecopy = manager.loadFile(copyTestFile, FileFormat.COMPRESSED);

            Assert.AreEqual(file.width, filecopy.width);
            Assert.AreEqual(file.width, filecopy.width);
            Assert.AreEqual(file.pixels.Count(), filecopy.pixels.Count());

            File.Delete(copyTestFile);
        }

    }
}
