using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace L3dtFileManager.Amf
{
    public class AmfPixelInfo
    {
        public uint x;
        public uint y;
        public byte landTypeId;
        public byte climateId;

        public AmfPixelInfo(uint x, uint y, byte landTypeId, byte climateId) 
        {
            this.x = x;
            this.y = y;
            this.landTypeId = landTypeId;
            this.climateId = climateId;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("x: " + this.x + "\n");
            builder.Append("y: " + this.y + "\n");
            builder.Append("landTypeId: " + this.landTypeId + "\n");
            builder.Append("climateId: " + this.climateId + "\n");
            return builder.ToString();
        }
    }
}
