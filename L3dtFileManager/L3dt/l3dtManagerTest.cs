using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace L3dtFileManager.L3dt
{
    [TestFixture]
    class l3dtManagerTest
    {
        private String pathToTestFile = "C:\\Git\\Repos\\L3DT_Filemanager\\L3dtFileManager";

        [Test]
        public void dmfLevelL3dtTest()
        {
            string fileName = pathToTestFile + "\\testfile\\l3dt_dmf_level\\l3dttest.proj";
            L3dtFileManager manager = new L3dtFileManager();
            L3dtFile result = manager.loadL3dtProject(fileName);
            Assert.NotNull(result.DmfFile);
            Assert.IsNull(result.HffFile);
            Assert.IsNull(result.HfzFile);
            Assert.IsNull(result.WmfFile);
            Assert.IsNull(result.AmfFile);
            Assert.True(result.ClimateTable.Count > 0);
        }

        [Test]
        public void hfLevelL3dtTest()
        {
            string fileName = pathToTestFile + "\\testfile\\l3dt_hf_level\\l3dttest.proj";
            L3dtFileManager manager = new L3dtFileManager();
            L3dtFile result = manager.loadL3dtProject(fileName);
            Assert.NotNull(result.DmfFile);
            Assert.IsNull(result.HffFile);
            Assert.NotNull(result.HfzFile);
            Assert.IsNull(result.WmfFile);
            Assert.IsNull(result.AmfFile);
            Assert.True(result.ClimateTable.Count > 0);
        }

        [Test]
        public void wmfLevelL3dtTest()
        {
            string fileName = pathToTestFile + "\\testfile\\l3dt_wmf_level\\l3dttest.proj";
            L3dtFileManager manager = new L3dtFileManager();
            L3dtFile result = manager.loadL3dtProject(fileName);
            Assert.NotNull(result.DmfFile);
            Assert.IsNull(result.HffFile);
            Assert.NotNull(result.HfzFile);
            Assert.NotNull(result.WmfFile);
            Assert.IsNull(result.AmfFile);
            Assert.True(result.ClimateTable.Count > 0);
        }

        [Test]
        public void amfLevelL3dtTest()
        {
            string fileName = pathToTestFile + "\\testfile\\l3dt_amf_level\\l3dttest.proj";
            L3dtFileManager manager = new L3dtFileManager();
            L3dtFile result = manager.loadL3dtProject(fileName);
            Assert.NotNull(result.DmfFile);
            Assert.IsNull(result.HffFile);
            Assert.NotNull(result.HfzFile);
            Assert.NotNull(result.WmfFile);
            Assert.NotNull(result.AmfFile);
            Assert.True(result.ClimateTable.Count > 0);
        }
    }
}
