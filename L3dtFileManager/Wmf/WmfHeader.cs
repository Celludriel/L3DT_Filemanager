using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace L3dtFileManager.Wmf
{
    public class WmfHeader
    {
        public ushort dataOffset;
        public uint width;
        public uint height;
        public byte waterLevelDataSize;
        public bool floatingPointFlag;
        public float verticalScale;
        public float verticalOffset;
        public float horizontalScale;
        public ushort tileSize;
        public bool wrapFlag;
        public byte reserved = 0;
        public ushort auxDataType;
        public byte auxDataSize;
        public byte[] auxReserved;

        public WmfHeader(ushort dataOffset, uint width, uint height, byte waterLevelDataSize,
            bool floatingPointFlag, float verticalScale, float verticalOffset, float horizontalScale,
            ushort tileSize, bool wrapFlag, ushort auxDataType, byte auxDataSize, byte[] auxReserved)
        {
            this.dataOffset = dataOffset;
            this.width = width;
            this.height = height;
            this.waterLevelDataSize = waterLevelDataSize;
            this.floatingPointFlag = floatingPointFlag;
            this.verticalScale = verticalScale;
            this.verticalOffset = verticalOffset;
            this.horizontalScale = horizontalScale;
            this.tileSize = tileSize;
            this.wrapFlag = wrapFlag;
            this.auxDataType = auxDataType;
            this.auxDataSize = auxDataSize;
            this.auxReserved = auxReserved;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("dataOffset: " + this.dataOffset + "\n");
            builder.Append("width: " + this.width + "\n");
            builder.Append("height: " + this.height + "\n");
            builder.Append("waterLevelDataSize: " + this.waterLevelDataSize + "\n");
            builder.Append("floatingPointFlag: " + this.floatingPointFlag + "\n");
            builder.Append("verticalScale: " + this.verticalScale + "\n");
            builder.Append("verticalOffset: " + this.verticalOffset + "\n");
            builder.Append("horizontalScale: " + this.horizontalScale + "\n");
            builder.Append("tileSize: " + this.tileSize + "\n");
            builder.Append("wrapFlag: " + this.wrapFlag + "\n");
            builder.Append("auxDataType: " + this.auxDataType + "\n");
            builder.Append("auxDataSize: " + this.auxDataSize + "\n");
            builder.Append("auxReserved: " + this.auxReserved.Length + "\n");
            return builder.ToString();
        }
    }
}
