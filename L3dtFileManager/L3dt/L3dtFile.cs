using L3dtFileManager.Amf;
using L3dtFileManager.Dmf;
using L3dtFileManager.Hff;
using L3dtFileManager.Hfz;
using L3dtFileManager.Wmf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace L3dtFileManager.L3dt
{
    public class L3dtFile
    {
        public enum HeightMapType { HFF, HFZ };

        public HeightMapType heightMapType;
        public HffFile HffFile;
        public HfzFile HfzFile;
        public WmfFile WmfFile;
        public AmfFile AmfFile;
        public DmfFile DmfFile;
        public Hashtable ClimateTable = new Hashtable();
    }
}
