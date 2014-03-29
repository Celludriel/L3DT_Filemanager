using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace L3dtFileManager.Hfz
{
    public class HfzTile
    {
        public UInt32 x;
        public UInt32 y;
        public UInt32 rowSize;
        public UInt32 rows;
        public List<float> tileData;

        public HfzTile(UInt32 x, UInt32 y, UInt32 rowSize, UInt32 rows, List<float> tileData)
        {
            if (tileData == null || tileData.Count == 0)
            {
                throw new Exception("tileData cannot be null or empty");
            }

            this.x = x;
            this.y = y;
            this.rowSize = rowSize;
            this.rows = rows;
            this.tileData = tileData;
        }
    }
}
