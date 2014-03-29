using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MiscUtil.Conversion;
using MiscUtil.IO;

namespace L3dtFileManager.Hfz
{
    public class HfzHeader
    {
        public ushort FileVersionNo = 0;
        public UInt32 nx = 0;
        public UInt32 ny = 0;
        public ushort TileSize = 256;
        public float HorizScale = 0.01f;
        public float Precis = 1.0f;
        public UInt32 ExtHeaderLength = 0;
        public UInt32 nExtHeaderBlocks = 0;
        public List<HfzExtHeaderBlock> pExtHeaderBlocks = null;

        public HfzHeader()
        {
            this.pExtHeaderBlocks = new List<HfzExtHeaderBlock>();
        }

        public uint countExtLength()
        {
            uint retValue = 0;
            if (pExtHeaderBlocks != null)
            {
                foreach (HfzExtHeaderBlock block in pExtHeaderBlocks)
                {
                    retValue = retValue + 24 + (uint)block.pBlockData.Length;
                }
            }
            return retValue;
        }

        public byte[] getByteArray()
        {
            MemoryStream stream = new MemoryStream();
            EndianBinaryWriter writer = Util.createEndianBinaryWriterForStream(stream);

            writer.Write("HF2\0");
            writer.Write(this.FileVersionNo);
            writer.Write(this.nx);
            writer.Write(this.ny);
            writer.Write(this.TileSize);
            writer.Write(this.HorizScale);
            writer.Write(this.Precis);
            writer.Write(this.ExtHeaderLength);

            foreach (HfzExtHeaderBlock block in this.pExtHeaderBlocks)
            {
                writer.Write(block.BlockType);
                writer.Write(block.BlockName);
                writer.Write(block.BlockLength);
                writer.Write(block.pBlockData);
            }

            return stream.ToArray();
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("FileVersionNo: " + this.FileVersionNo + "\n");
            builder.Append("nx: " + this.nx + "\n");
            builder.Append("ny: " + this.ny + "\n");
            builder.Append("TileSize: " + this.TileSize + "\n");
            builder.Append("HorizScale: " + this.HorizScale + "\n");
            builder.Append("Precis: " + this.Precis + "\n");
            builder.Append("ExtHeaderLength: " + this.ExtHeaderLength + "\n");
            builder.Append("nExtHeaderBlocks: " + this.nExtHeaderBlocks + "\n");
            builder.Append("pExtHeaderBlocks: " + this.pExtHeaderBlocks + "\n");
            return builder.ToString();
        }
    }
}