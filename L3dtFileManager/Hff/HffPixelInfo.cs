using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace L3dtFileManager.Hff
{
    public class HffPixelInfo
    {
        public uint x;
        public uint y;
        public float data;

        public HffPixelInfo(uint x, uint y, float data) 
        {
            this.x = x;
            this.y = y;
            this.data = data;
        }
    }
}
