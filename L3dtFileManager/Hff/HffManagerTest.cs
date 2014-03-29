using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace L3dtFileManager.Hff
{
    using NUnit.Framework;

    [TestFixture]
    class HffManagerTest
    {
        //change this to your path before running the test
        private String pathToTestFile = "c:\\users\\n069261kds\\documents\\visual studio 2013\\Projects\\L3dtFileManager\\ClassLibrary1";

        [Test]
        public void HffUnCompressedUnTiledReadTest()
        {
            HffManager manager = new HffManager();
            HffFile file = manager.loadFile(pathToTestFile + "\\testfile\\test_untiled.hff");
            
            HffHeader header = file.header;
            Assert.AreEqual(64, header.dataOffset);
            Assert.AreEqual(1024, header.width);
            Assert.AreEqual(512, header.height);
            Assert.AreEqual(2, header.dataSize);
            Assert.AreEqual(false, header.floatingPointFlag);
            Assert.AreEqual(0.0162765328f, header.verticalScale);
            Assert.AreEqual(-47.0971947f, header.verticalOffset);
            Assert.AreEqual(10, header.horizontalScale);
            Assert.AreEqual(0, header.tileSize);
            Assert.AreEqual(false, header.wrapFlag);
            Assert.AreEqual(23, header.reserved.Length);
            Assert.AreEqual(524288, file.pixels.Count());
        }

        [Test]
        public void HffUnCompressedTiledReadTest()
        {
            HffManager manager = new HffManager();
            HffFile file = manager.loadFile(pathToTestFile + "\\testfile\\test_512_tiled.hff");

            HffHeader header = file.header;
            Assert.AreEqual(64, header.dataOffset);
            Assert.AreEqual(1024, header.width);
            Assert.AreEqual(512, header.height);
            Assert.AreEqual(2, header.dataSize);
            Assert.AreEqual(false, header.floatingPointFlag);
            Assert.AreEqual(0.0162765328f, header.verticalScale);
            Assert.AreEqual(-47.0971947f, header.verticalOffset);
            Assert.AreEqual(10, header.horizontalScale);
            Assert.AreEqual(512, header.tileSize);
            Assert.AreEqual(false, header.wrapFlag);
            Assert.AreEqual(23, header.reserved.Length);
            Assert.AreEqual(524288, file.pixels.Count());
        }

        [Test]
        public void HffUnCompressedUnTiledWriteTest()
        {
            HffManager manager = new HffManager();
            string copyFilePath = pathToTestFile + "\\testfile\\copy_test_untiled.hff";
            HffFile file = manager.loadFile(pathToTestFile + "\\testfile\\test_untiled.hff");          
            manager.saveFile(copyFilePath, file);
            HffFile copyfile = manager.loadFile(copyFilePath);

            HffHeader header = file.header;
            HffHeader copyHeader = copyfile.header;
            Assert.AreEqual(header.dataOffset, copyHeader.dataOffset);
            Assert.AreEqual(header.width, copyHeader.width);
            Assert.AreEqual(header.height, copyHeader.height);
            Assert.AreEqual(header.dataSize, copyHeader.dataSize);
            Assert.AreEqual(header.floatingPointFlag, copyHeader.floatingPointFlag);
            Assert.AreEqual(header.verticalScale, copyHeader.verticalScale);
            Assert.AreEqual(header.verticalOffset, copyHeader.verticalOffset);
            Assert.AreEqual(header.horizontalScale, copyHeader.horizontalScale);
            Assert.AreEqual(header.tileSize, copyHeader.tileSize);
            Assert.AreEqual(header.wrapFlag, copyHeader.wrapFlag);
            Assert.AreEqual(header.reserved, copyHeader.reserved);
            Assert.AreEqual(file.pixels.Count(), copyfile.pixels.Count());

            File.Delete(copyFilePath);
        }

        [Test]
        public void HffUnCompressedTiledWriteTest()
        {
            HffManager manager = new HffManager();
            string copyFilePath = pathToTestFile + "\\testfile\\copy_test_tiled.hff";
            HffFile file = manager.loadFile(pathToTestFile + "\\testfile\\test_512_tiled.hff");
            manager.saveFile(copyFilePath, file);
            HffFile copyfile = manager.loadFile(copyFilePath);

            HffHeader header = file.header;
            HffHeader copyHeader = copyfile.header;
            Assert.AreEqual(header.dataOffset, copyHeader.dataOffset);
            Assert.AreEqual(header.width, copyHeader.width);
            Assert.AreEqual(header.height, copyHeader.height);
            Assert.AreEqual(header.dataSize, copyHeader.dataSize);
            Assert.AreEqual(header.floatingPointFlag, copyHeader.floatingPointFlag);
            Assert.AreEqual(header.verticalScale, copyHeader.verticalScale);
            Assert.AreEqual(header.verticalOffset, copyHeader.verticalOffset);
            Assert.AreEqual(header.horizontalScale, copyHeader.horizontalScale);
            Assert.AreEqual(header.tileSize, copyHeader.tileSize);
            Assert.AreEqual(header.wrapFlag, copyHeader.wrapFlag);
            Assert.AreEqual(header.reserved, copyHeader.reserved);
            Assert.AreEqual(file.pixels.Count(), copyfile.pixels.Count());

            File.Delete(copyFilePath);
        }

    }
}
