using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using L3dtFileManager.Hfz;
using L3dtFileManager.Hff;
using L3dtFileManager.Amf;
using L3dtFileManager.Dmf;
using L3dtFileManager.Wmf;

namespace L3dtFileManager
{
    public enum FileFormat { UNCOMPRESSED, COMPRESSED };

    public class L3dtFileManager
    {
        private HfzManager hfzManager = new HfzManager();
        private AmfManager amfManager = new AmfManager();
        private DmfManager dmfManager = new DmfManager();
        private HffManager hffManager = new HffManager();
        private WmfManager wmfManager = new WmfManager();

        public HfzFile loadHfzFile(string fileName, FileFormat format)
        {
            return hfzManager.loadFile(fileName, format);
        }

        public void saveHfzFile(string fileName, FileFormat format, HfzFile file)
        {
            hfzManager.saveFile(fileName, format, file);
        }

        public AmfFile loadAmfFile(string fileName, FileFormat format)
        {
            return amfManager.loadFile(fileName, format);
        }

        public void saveAmfFile(string fileName, FileFormat format, AmfFile file)
        {
            amfManager.saveFile(fileName, format, file);
        }

        public DmfFile loadDmfFile(string fileName)
        {
            return dmfManager.loadFile(fileName);
        }

        public void saveDmfFile(string fileName, DmfFile file)
        {
            dmfManager.saveFile(fileName, file);
        }

        public HffFile loadHffFile(string fileName)
        {
            return hffManager.loadFile(fileName);
        }

        public void saveHffFile(string fileName, HffFile file)
        {
            hffManager.saveFile(fileName, file);
        }

        public WmfFile loadWmfFile(string fileName)
        {
            return wmfManager.loadFile(fileName);
        }

        public void saveWmfFile(string fileName, WmfFile file)
        {
            wmfManager.saveFile(fileName, file);
        }

    }
}
