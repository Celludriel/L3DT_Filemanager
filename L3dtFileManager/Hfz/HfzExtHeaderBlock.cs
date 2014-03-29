using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace L3dtFileManager.Hfz
{
    public class HfzExtHeaderBlock
    {
        public string BlockType;
        public string BlockName;
        public UInt32 BlockLength;
        public byte[] pBlockData;

        public HfzExtHeaderBlock(string lpBlockType, string lpBlockName, UInt32 BlockDataLength, byte[] pBlockData)
        {
            init(lpBlockType, lpBlockName, BlockDataLength, pBlockData);
        }

        public void init(string lpBlockType, string lpBlockName, UInt32 BlockDataLength, byte[] pBlockData)
        {
            this.BlockType = null;
            this.BlockName = null;

            if (lpBlockType != null)
            {
                if (lpBlockType.Length > 4)
                {
                    throw new Exception("Invalid parameter lpBlockType shouldn't exceed 3 length: " + lpBlockType + " ,size: " + lpBlockType.Length);
                }

                this.BlockType = lpBlockType;
            }

            if (lpBlockName != null)
            {
                if (lpBlockName.Length > 16)
                {
                    throw new Exception("Invalid parameter lpBlockName shouldn't exceed 16 length: " + lpBlockName + " ,size: " + lpBlockName.Length);
                }

                this.BlockName = lpBlockName;
            }

            this.BlockLength = BlockDataLength;
            if (this.BlockLength == 0)
            {
                this.pBlockData = null;
            }
            else
            {
                this.pBlockData = pBlockData;
            }
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("BlockType: ");
            builder.Append(this.BlockType + "\n");
            builder.Append("BlockName: ");
            builder.Append(this.BlockName + "\n");
            builder.Append("BlockLength: " + this.BlockLength + "\n");
            builder.Append("pBlockData: " + this.pBlockData + "\n");
            builder.Replace("\0", null);
            return builder.ToString();
        }
    }
}