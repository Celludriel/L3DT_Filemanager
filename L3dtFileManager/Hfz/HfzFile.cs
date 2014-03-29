using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MiscUtil.Conversion;
using MiscUtil.IO;

namespace L3dtFileManager.Hfz
{
    public class HfzFile
    {
        public HfzHeader header = null;
        public Dictionary<short, HfzTile> mapData = null;

        public float getPixelAt(uint x, uint y) 
        { 
            // find the tile the pixel is at
            HfzTile tile = getTileForPixel(x, y);
            // index formula = ((((y - (TileYCoord * TileSize)) - 1) * rowsize) + (x - (TileXCoord * TileSize)))
            uint rows = tile.rows;
            uint rowsize = tile.rowSize;
            int pixelIndex = -1;
            try
            {
                pixelIndex = (int)((((y - (tile.y * header.TileSize)) - 1) * tile.rowSize) + (x - (tile.x * header.TileSize))-1);
                return tile.tileData.ElementAt(pixelIndex);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("X: " + x);
                System.Console.WriteLine("Y: " + y);
                System.Console.WriteLine("Tile: " + tile.x + "," + tile.y);
                System.Console.WriteLine("rows: " + rows);
                System.Console.WriteLine("rowsize: " + rowsize);
                System.Console.WriteLine("PixelIndex: " + pixelIndex);
                throw ex;
            }
        }

        public float getMaxHeight()
        {
            Nullable<float> maxHeight = null;
            Epsilon eps = new Epsilon(0.01);
            List<short> keys = getTileKeys();
            foreach (short key in keys)
            {
                HfzTile tile = mapData.ElementAt(key).Value;
                List<float> tileData = tile.tileData;
                foreach (float data in tileData)
                {
                    if (!maxHeight.HasValue || RealExtensions.GT(data, maxHeight.Value, eps))
                    {
                        maxHeight = data;
                    }
                }
            }
            return maxHeight.Value;
        }

        public float getMinHeight()
        {
            Nullable<float> minHeight = null;
            Epsilon eps = new Epsilon(0.01);
            List<short> keys = getTileKeys();
            foreach (short key in keys)
            {
                HfzTile tile = mapData.ElementAt(key).Value;
                List<float> tileData = tile.tileData;
                foreach (float data in tileData)
                {
                    if (!minHeight.HasValue || RealExtensions.LT(data, minHeight.Value, eps))
                    {
                        minHeight = data;
                    }
                }
            }
            return minHeight.Value;
        }

        public HfzTile getTileData(short tileNo)
        {
            List<short> keys = mapData.Keys.ToList();
            foreach (short key in keys)
            {
                if (key == tileNo)
                {
                    return mapData[key];
                }
            }
            return null;
        }

        public void addTile(short tileNo, HfzTile tile)
        {
            mapData.Add(tileNo, tile);
        }

        public List<short> getTileKeys()
        {
            if (mapData != null)
            {
                return mapData.Keys.ToList();
            }
            return null;
        } 

        public byte[] getByteArray()
        {
            if (header != null && mapData != null)
            {
                MemoryStream stream = new MemoryStream();
                EndianBinaryWriter writer = Util.createEndianBinaryWriterForStream(stream);

                writer.Write(header.getByteArray());
                List<short> keys = mapData.Keys.ToList();
                foreach (short key in keys)
                {
                    HfzTile data = mapData[key];
                    List<float> elements = data.tileData;
                    foreach (float element in elements)
                    {
                        writer.Write(element);
                    }
                }
                return stream.ToArray();
            }
            else
            {
                return null;
            }
        }

        public UInt32 byteCount()
        {
            UInt32 count = 0;
            if (mapData != null)
            {
                List<short> keys = mapData.Keys.ToList();

                foreach (short key in keys)
                {
                    HfzTile data = mapData[key];
                    count += (uint)(data.tileData.Count * 4);
                }
            }
            return count;
        }

        private HfzTile getTileForPixel(uint x, uint y)
        {
            ushort tileSize = header.TileSize;
            uint tileX = (uint)Math.Floor((x-1) / (double)tileSize);
            uint tileY = (uint)Math.Floor((y-1) / (double)tileSize);            

            uint nx = header.nx;
            uint ny = header.ny;

            UInt32 nBlocksX = nx / tileSize;
            UInt32 nBlocksY = ny / tileSize;
            if (nx % tileSize != 0)
            {
                nBlocksX++;                
            }

            if (ny % tileSize != 0)
            {
                nBlocksY++;               
            }

            uint tileIndex = (tileY * nBlocksX) + tileX;
            return mapData.ElementAt((int)tileIndex).Value;
        }
        
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            if (header != null)
            {
                builder.Append(header);
                if (header.pExtHeaderBlocks != null)
                {
                    if (header.pExtHeaderBlocks.Count > 0)
                    {
                        foreach (HfzExtHeaderBlock block in header.pExtHeaderBlocks)
                        {
                            builder.Append(block);
                        }
                    }
                }
            }

            if(mapData != null)
            {
                List<short> keys = mapData.Keys.ToList();

                builder.Append("mapData amount tiles : " + keys.Count + "\n");

                Int32 datacount = 0;
                foreach (short key in keys)
                {
                    HfzTile data = mapData[key];
                    datacount += (data.tileData.Count * 4);
                }

                builder.Append("full amount of data : " + datacount + " bytes");
            }
            return builder.ToString();
        }
    }
}
