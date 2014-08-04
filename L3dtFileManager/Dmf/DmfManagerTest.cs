using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace L3dtFileManager.Dmf
{
    using NUnit.Framework;

    [TestFixture]
    class DmfManagerTest
    {
        //change this to your path before running the test
        private String pathToTestFile = "C:\\Git\\Repos\\L3DT_Filemanager\\L3dtFileManager";

        [Test]
        public void DmfUnCompressedReadTest()
        {
            DmfManager manager = new DmfManager();
            DmfFile file = manager.loadFile(pathToTestFile + "\\testfile\\test_DM.dmf");

            Assert.AreEqual(32, file.width);
            Assert.AreEqual(32, file.height);
            Assert.AreEqual(file.width * file.height, file.pixels.Count());
        }

        [Test]
        public void DmfGetPixelAtTest()
        {
            DmfManager manager = new DmfManager();
            DmfFile file = manager.loadFile(pathToTestFile + "\\testfile\\test_DM.dmf");

            DmfPixelInfo value = file.getPixelAt(10, 10);

            Assert.AreEqual(0, value.altitude);
            Assert.AreEqual(75, value.peakRoughness);
            Assert.AreEqual(115, value.fractalRoughness);
            Assert.AreEqual(74, value.cliffStrength);
            Assert.AreEqual(0, value.erosionStrength);
            Assert.AreEqual(0, value.autoLakeStrength);
            Assert.AreEqual(1, value.climateId);
            Assert.AreEqual(0, value.specialTypeId);
            Assert.AreEqual(0, value.specialTypeParam);
        }

        [Test]
        [ExpectedException]
        public void DmfBadMarkerReadTest()
        {
            DmfManager manager = new DmfManager();
            DmfFile file = manager.loadFile(pathToTestFile + "\\testfile\\test_DM_bad_marker.dmf");
        }

        [Test]
        [ExpectedException]
        public void DmfBadFileTypeReadTest()
        {
            DmfManager manager = new DmfManager();
            DmfFile file = manager.loadFile(pathToTestFile + "\\testfile\\test_DM_bad_file_type.dmf");
        }

        [Test]
        public void DmfUnCompressedWriteTest()
        {
            string copyTestFile = pathToTestFile + "\\testfile\\filecopy.amf";
            DmfManager manager = new DmfManager();
            DmfFile file = manager.loadFile(pathToTestFile + "\\testfile\\test_DM.dmf");
            manager.saveFile(copyTestFile, file);
            DmfFile filecopy = manager.loadFile(copyTestFile);

            Assert.AreEqual(file.width, filecopy.width);
            Assert.AreEqual(file.width, filecopy.width);
            Assert.AreEqual(file.pixels.Count(), filecopy.pixels.Count());

            File.Delete(copyTestFile);
        }        
    }
}
