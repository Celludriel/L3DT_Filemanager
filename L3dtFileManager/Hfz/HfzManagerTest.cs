using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MiscUtil.Conversion;
using MiscUtil.IO;

namespace L3dtFileManager.Hfz
{
    using NUnit.Framework;

    [TestFixture]
    class HfzManagerTest
    {
        //change this to your path before running the test
        private String pathToTestFile = "C:\\Git\\Repos\\L3DT_Filemanager\\L3dtFileManager";

        [Test]
        public void LibHfzUnCompressedReadTest()
        {
            HfzManager lib = new HfzManager();
            HfzFile file1 = lib.loadFile(pathToTestFile + "\\testfile\\baia_mare_HF.hf2", FileFormat.UNCOMPRESSED);
            HfzFile file2 = lib.loadFile(pathToTestFile + "\\testfile\\demo2_HF.hf2", FileFormat.UNCOMPRESSED);
            HfzFile file3 = lib.loadFile(pathToTestFile + "\\testfile\\fractal-16bit.hf2", FileFormat.UNCOMPRESSED);
            HfzFile file4 = lib.loadFile(pathToTestFile + "\\testfile\\temp29_HF.hf2", FileFormat.UNCOMPRESSED);
            HfzFile file5 = lib.loadFile(pathToTestFile + "\\testfile\\MtStHelens_10m.hf2", FileFormat.UNCOMPRESSED);

            Assert.AreEqual(16793604, file1.byteCount());
            Assert.AreEqual(2097152, file2.byteCount());
            Assert.AreEqual(4194304, file3.byteCount());
            Assert.AreEqual(1048576, file4.byteCount());
            Assert.AreEqual(5163868, file5.byteCount());
        }

        [Test]
        public void LibHfzUnCompressedWriteTest()
        {
            HfzManager lib = new HfzManager();
            String copyTestFile = pathToTestFile + "\\testfile\\copytest.hf2";
            uint byteCount = 0;
            HfzFile file = null;

            file = lib.loadFile(pathToTestFile + "\\testfile\\baia_mare_HF.hf2", FileFormat.UNCOMPRESSED);
            byteCount = file.byteCount();
            lib.saveFile(copyTestFile, FileFormat.UNCOMPRESSED, file);
            file = lib.loadFile(copyTestFile, FileFormat.UNCOMPRESSED);
            Assert.AreEqual(byteCount, file.byteCount());

            file = lib.loadFile(pathToTestFile + "\\testfile\\demo2_HF.hf2", FileFormat.UNCOMPRESSED);
            byteCount = file.byteCount();
            lib.saveFile(copyTestFile, FileFormat.UNCOMPRESSED, file);
            file = lib.loadFile(copyTestFile, FileFormat.UNCOMPRESSED);
            Assert.AreEqual(byteCount, file.byteCount());

            file = lib.loadFile(pathToTestFile + "\\testfile\\fractal-16bit.hf2", FileFormat.UNCOMPRESSED);
            byteCount = file.byteCount();
            lib.saveFile(copyTestFile, FileFormat.UNCOMPRESSED, file);
            file = lib.loadFile(copyTestFile, FileFormat.UNCOMPRESSED);
            Assert.AreEqual(byteCount, file.byteCount());

            file = lib.loadFile(pathToTestFile + "\\testfile\\temp29_HF.hf2", FileFormat.UNCOMPRESSED);
            byteCount = file.byteCount();
            lib.saveFile(copyTestFile, FileFormat.UNCOMPRESSED, file);
            file = lib.loadFile(copyTestFile, FileFormat.UNCOMPRESSED);
            Assert.AreEqual(byteCount, file.byteCount());

            file = lib.loadFile(pathToTestFile + "\\testfile\\MtStHelens_10m.hf2", FileFormat.UNCOMPRESSED);
            byteCount = file.byteCount();
            lib.saveFile(copyTestFile, FileFormat.UNCOMPRESSED, file);
            file = lib.loadFile(copyTestFile, FileFormat.UNCOMPRESSED);
            Assert.AreEqual(byteCount, file.byteCount());

            File.Delete(copyTestFile);
        }

        [Test]
        public void LibHfzCompressedReadTest()
        {
            HfzManager lib = new HfzManager();
            HfzFile file1 = lib.loadFile(pathToTestFile + "\\testfile\\baia_mare_HF.hfz", FileFormat.COMPRESSED);
            HfzFile file2 = lib.loadFile(pathToTestFile + "\\testfile\\demo2_HF.hf2.gz", FileFormat.COMPRESSED);
            HfzFile file3 = lib.loadFile(pathToTestFile + "\\testfile\\fractal-16bit.hf2.gz", FileFormat.COMPRESSED);
            HfzFile file4 = lib.loadFile(pathToTestFile + "\\testfile\\temp29_HF.hf2.gz", FileFormat.COMPRESSED);
            HfzFile file5 = lib.loadFile(pathToTestFile + "\\testfile\\MtStHelens_10m.hfz", FileFormat.COMPRESSED);

            Assert.AreEqual(16793604, file1.byteCount());
            Assert.AreEqual(2097152, file2.byteCount());
            Assert.AreEqual(4194304, file3.byteCount());
            Assert.AreEqual(1048576, file4.byteCount());
            Assert.AreEqual(5163868, file5.byteCount());
        }

        [Test]
        public void LibHfzCompressedWriteTest()
        {
            HfzManager lib = new HfzManager();
            String copyTestFile = pathToTestFile + "\\testfile\\copytest.hf2";
            uint byteCount = 0;
            HfzFile file = null;

            file = lib.loadFile(pathToTestFile + "\\testfile\\baia_mare_HF.hfz", FileFormat.COMPRESSED);
            byteCount = file.byteCount();
            lib.saveFile(copyTestFile, FileFormat.COMPRESSED, file);
            file = lib.loadFile(copyTestFile, FileFormat.COMPRESSED);
            Assert.AreEqual(byteCount, file.byteCount());

            file = lib.loadFile(pathToTestFile + "\\testfile\\demo2_HF.hf2.gz", FileFormat.COMPRESSED);
            byteCount = file.byteCount();
            lib.saveFile(copyTestFile, FileFormat.COMPRESSED, file);
            file = lib.loadFile(copyTestFile, FileFormat.COMPRESSED);
            Assert.AreEqual(byteCount, file.byteCount());

            file = lib.loadFile(pathToTestFile + "\\testfile\\fractal-16bit.hf2.gz", FileFormat.COMPRESSED);
            byteCount = file.byteCount();
            lib.saveFile(copyTestFile, FileFormat.COMPRESSED, file);
            file = lib.loadFile(copyTestFile, FileFormat.COMPRESSED);
            Assert.AreEqual(byteCount, file.byteCount());

            file = lib.loadFile(pathToTestFile + "\\testfile\\temp29_HF.hf2.gz", FileFormat.COMPRESSED);
            byteCount = file.byteCount();
            lib.saveFile(copyTestFile, FileFormat.COMPRESSED, file);
            file = lib.loadFile(copyTestFile, FileFormat.COMPRESSED);
            Assert.AreEqual(byteCount, file.byteCount());

            file = lib.loadFile(pathToTestFile + "\\testfile\\MtStHelens_10m.hfz", FileFormat.COMPRESSED);
            byteCount = file.byteCount();
            lib.saveFile(copyTestFile, FileFormat.COMPRESSED, file);
            file = lib.loadFile(copyTestFile, FileFormat.COMPRESSED);
            Assert.AreEqual(byteCount, file.byteCount());

            File.Delete(copyTestFile);
        }

        [Test]
        [ExpectedException]
        public void HfzTileDataNotNull()
        {
            HfzTile tile = new HfzTile(0, 0, 0, 0, null);
        }

        [Test]
        [ExpectedException]
        public void HfzTileDataNotEmpty()
        {
            HfzTile tile = new HfzTile(0, 0, 0, 0, new List<float>());
        }

        [Test]
        public void HfzTileCreation()
        {
            List<float> data = new List<float>();
            data.Add(1.0f);
            HfzTile tile = new HfzTile(0, 0, 0, 0, data);
            Assert.NotNull(tile);
        }

        [Test]
        public void getPixelAtBottomLeftTest()
        {
            HfzManager lib = new HfzManager();
            HfzFile file = lib.loadFile(pathToTestFile + "\\testfile\\MtStHelens_10m.hfz", FileFormat.COMPRESSED);
            System.Console.WriteLine(file.header.nx + " - " + file.header.ny);
            float data = file.getPixelAt(1, 1);
            System.Console.WriteLine(data);
        }

        [Test]
        public void getPixelAtBottomRightTest()
        {
            HfzManager lib = new HfzManager();
            HfzFile file = lib.loadFile(pathToTestFile + "\\testfile\\MtStHelens_10m.hfz", FileFormat.COMPRESSED);
            float data = file.getPixelAt(943, 1);
            System.Console.WriteLine(data);
        }

        [Test]
        public void getPixelAtTopLeftTest()
        {
            HfzManager lib = new HfzManager();
            HfzFile file = lib.loadFile(pathToTestFile + "\\testfile\\MtStHelens_10m.hfz", FileFormat.COMPRESSED);

            float data = file.getPixelAt(1, 1369);
            System.Console.WriteLine(data);
        }

        [Test]
        public void getPixelAtTopRightTest()
        {
            HfzManager lib = new HfzManager();
            HfzFile file = lib.loadFile(pathToTestFile + "\\testfile\\MtStHelens_10m.hfz", FileFormat.COMPRESSED);

            float data = file.getPixelAt(943, 1369);
            System.Console.WriteLine(data);
        }

        [Test]
        public void getAllPixelTest()
        {
            HfzManager lib = new HfzManager();
            HfzFile file = lib.loadFile(pathToTestFile + "\\testfile\\MtStHelens_10m.hfz", FileFormat.COMPRESSED);
            checkAllPixels(file, (int)(file.header.nx*file.header.ny));

            file = lib.loadFile(pathToTestFile + "\\testfile\\demo2_HF.hf2.gz", FileFormat.COMPRESSED);
            checkAllPixels(file, (int)(file.header.nx * file.header.ny));

            file = lib.loadFile(pathToTestFile + "\\testfile\\fractal-16bit.hf2.gz", FileFormat.COMPRESSED);
            checkAllPixels(file, (int)(file.header.nx * file.header.ny));

            file = lib.loadFile(pathToTestFile + "\\testfile\\temp29_HF.hf2.gz", FileFormat.COMPRESSED);
            checkAllPixels(file, (int)(file.header.nx * file.header.ny));

            file = lib.loadFile(pathToTestFile + "\\testfile\\baia_mare_HF.hfz", FileFormat.COMPRESSED);
            checkAllPixels(file, (int)(file.header.nx * file.header.ny));
        }

        [Test]
        public void getMaximumTest()
        {
            HfzManager lib = new HfzManager();
            HfzFile file = lib.loadFile(pathToTestFile + "\\testfile\\MtStHelens_10m.hfz", FileFormat.COMPRESSED);

            float max = file.getMaxHeight();
            Assert.AreEqual(2550.25391f, max);
        }

        [Test]
        public void getMinimumTest()
        {
            HfzManager lib = new HfzManager();
            HfzFile file = lib.loadFile(pathToTestFile + "\\testfile\\MtStHelens_10m.hfz", FileFormat.COMPRESSED);

            float min = file.getMinHeight();
            Assert.AreEqual(690.0672f, min);
        }

        private static void checkAllPixels(HfzFile file, int expectedCount)
        {
            int count = 0;
            for (uint i = 1; i <= file.header.nx; i++)
            {
                for (uint j = 1; j <= file.header.ny; j++)
                {
                    file.getPixelAt(i, j);
                    count++;
                }
            }

            Assert.AreEqual(expectedCount, count);
        }
    }
}
