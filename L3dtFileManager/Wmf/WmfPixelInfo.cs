using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace L3dtFileManager.Wmf
{
    public class WmfPixelInfo
    {
        public uint x;
        public uint y;
        public float data;
        public byte waterTypeId;
        public ushort waterBodyIndex;

        public WmfPixelInfo(uint x, uint y, float data, byte waterTypeId, ushort waterBodyIndex)
        {
            this.x = x;
            this.y = y;
            this.data = data;
            this.waterTypeId = waterTypeId;
            this.waterBodyIndex = waterBodyIndex;
        }

        public WmfPixelInfo(uint x, uint y, float data)
        {
            this.x = x;
            this.y = y;
            this.data = data;
        }
    }
}
