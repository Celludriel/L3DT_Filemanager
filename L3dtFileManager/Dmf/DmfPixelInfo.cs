using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace L3dtFileManager.Dmf
{
    public class DmfPixelInfo
    {
        public uint x;
        public uint y;
        public short altitude;
        public byte peakRoughness;
        public byte fractalRoughness;
        public byte cliffStrength;
        public byte erosionStrength;
        public byte autoLakeStrength;
        public byte climateId;
        public byte specialTypeId;
        public byte specialTypeParam;

        public DmfPixelInfo(uint x, uint y, short altitude, byte peakRoughness,byte fractalRoughness, byte cliffStrength, 
            byte erosionStrength, byte autoLakeStrength, byte climateId, byte specialTypeId, byte specialTypeParam)
        {
            this.x = x;
            this.y = y;
            this.altitude = altitude;
            this.peakRoughness = peakRoughness;
            this.fractalRoughness = fractalRoughness;
            this.cliffStrength = cliffStrength;
            this.erosionStrength = erosionStrength;
            this.autoLakeStrength = autoLakeStrength;
            this.climateId = climateId;
            this.specialTypeId = specialTypeId;
            this.specialTypeParam = specialTypeParam;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("x: " + this.x + "\n");
            builder.Append("y: " + this.y + "\n");
            builder.Append("altitude: " + this.altitude + "\n");
            builder.Append("peakRoughness: " + this.peakRoughness + "\n");
            builder.Append("fractalRoughness: " + this.fractalRoughness + "\n");
            builder.Append("cliffStrength: " + this.cliffStrength + "\n");
            builder.Append("erosionStrength: " + this.erosionStrength + "\n");
            builder.Append("autoLakeStrength: " + this.autoLakeStrength + "\n");
            builder.Append("climateId: " + this.climateId + "\n");
            builder.Append("specialTypeId: " + this.specialTypeId + "\n");
            builder.Append("specialTypeParam: " + this.specialTypeParam + "\n");
            return builder.ToString();
        }
    }
}
