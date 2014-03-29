using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace L3dtFileManager.Hff
{
    public class HffHeader
    {
        public ushort dataOffset;
        public uint width;
        public uint height;
        public byte dataSize;
        public bool floatingPointFlag;
        public float verticalScale;
        public float verticalOffset;
        public float horizontalScale;
        public ushort tileSize;
        public bool wrapFlag;
        public byte[] reserved;

        public HffHeader(ushort dataOffset, uint width, uint height, byte dataSize,
            bool floatingPointFlag, float verticalScale, float verticalOffset, float horizontalScale,
            ushort tileSize, bool wrapFlag, byte[] reserved)
        {
            this.dataOffset = dataOffset;
            this.width = width;
            this.height = height;
            this.dataSize = dataSize;
            this.floatingPointFlag = floatingPointFlag;
            this.verticalScale = verticalScale;
            this.verticalOffset = verticalOffset;
            this.horizontalScale = horizontalScale;
            this.tileSize = tileSize;
            this.wrapFlag = wrapFlag;
            this.reserved = reserved;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("dataOffset: " + this.dataOffset + "\n");
            builder.Append("width: " + this.width + "\n");
            builder.Append("height: " + this.height + "\n");
            builder.Append("dataSize: " + this.dataSize + "\n");
            builder.Append("floatingPointFlag: " + this.floatingPointFlag + "\n");
            builder.Append("verticalScale: " + this.verticalScale + "\n");
            builder.Append("verticalOffset: " + this.verticalOffset + "\n");
            builder.Append("horizontalScale: " + this.horizontalScale + "\n");
            builder.Append("tileSize: " + this.tileSize + "\n");
            builder.Append("wrapFlag: " + this.wrapFlag + "\n");
            builder.Append("reserved: " + this.reserved.Length + "\n");
            return builder.ToString();
        }
    }
}
