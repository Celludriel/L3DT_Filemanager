using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace L3dtFileManager.Wmf
{
    using NUnit.Framework;

    [TestFixture]
    class WmfManagerTest
    {
        //change this to your path before running the test
        private String pathToTestFile = "C:\\Git\\Repos\\L3DT_Filemanager\\L3dtFileManager";

        [Test]
        public void WmfUnCompressedUnTiledReadTest()
        {
            WmfManager manager = new WmfManager();
            WmfFile file = manager.loadFile(pathToTestFile + "\\testfile\\test_WM_untiled.wmf");

            WmfHeader header = file.header;
            Assert.AreEqual(64, header.dataOffset);
            Assert.AreEqual(1024, header.width);
            Assert.AreEqual(1024, header.height);
            Assert.AreEqual(2, header.waterLevelDataSize);
            Assert.AreEqual(false, header.floatingPointFlag);
            Assert.AreEqual(0.0042061517f, header.verticalScale);
            Assert.AreEqual(-136.315125f, header.verticalOffset);
            Assert.AreEqual(10, header.horizontalScale);
            Assert.AreEqual(0, header.tileSize);
            Assert.AreEqual(false, header.wrapFlag);
            Assert.AreEqual(0, header.reserved);
            Assert.AreEqual(1, header.auxDataType);
            Assert.AreEqual(3, header.auxDataSize);
            Assert.AreEqual(19, header.auxReserved.Length);
            Assert.AreEqual(1048576, file.pixels.Count());
        }

        [Test]
        public void WmfUnCompressedUnTiledWriteTest()
        {
            WmfManager manager = new WmfManager();
            string copyFilePath = pathToTestFile + "\\testfile\\copy_test_WM_untiled.wmf";
            WmfFile file = manager.loadFile(pathToTestFile + "\\testfile\\test_WM_untiled.wmf");
            manager.saveFile(copyFilePath, file);
            WmfFile copyfile = manager.loadFile(copyFilePath);

            WmfHeader header = file.header;
            WmfHeader copyHeader = copyfile.header;
            Assert.AreEqual(header.dataOffset, copyHeader.dataOffset);
            Assert.AreEqual(header.width, copyHeader.width);
            Assert.AreEqual(header.height, copyHeader.height);
            Assert.AreEqual(header.waterLevelDataSize, copyHeader.waterLevelDataSize);
            Assert.AreEqual(header.floatingPointFlag, copyHeader.floatingPointFlag);
            Assert.AreEqual(header.verticalScale, copyHeader.verticalScale);
            Assert.AreEqual(header.verticalOffset, copyHeader.verticalOffset);
            Assert.AreEqual(header.horizontalScale, copyHeader.horizontalScale);
            Assert.AreEqual(header.tileSize, copyHeader.tileSize);
            Assert.AreEqual(header.wrapFlag, copyHeader.wrapFlag);
            Assert.AreEqual(header.reserved, copyHeader.reserved);
            Assert.AreEqual(header.auxDataType, copyHeader.auxDataType);
            Assert.AreEqual(header.auxDataSize, copyHeader.auxDataSize);
            Assert.AreEqual(file.pixels.Count(), copyfile.pixels.Count());

            File.Delete(copyFilePath);
        }
    }
}
