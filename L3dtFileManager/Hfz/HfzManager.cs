using MiscUtil.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace L3dtFileManager.Hfz
{
    public class HfzManager:AbstractManager
    {
        public HfzFile loadFile(string fileName, FileFormat format)
        {
            HfzFile file = new HfzFile();

            Stream fs = null;
            EndianBinaryReader reader = null;
            try
            {
                debugLine("File format: " + format + ", file name: " + fileName);
                //create a file stream to work with during load
                fs = Util.openStream(fileName, format, FileMode.Open);
                reader = Util.createEndianBinaryReaderForStream(fs);

                HfzHeader header = hfzReadHeader(reader);
                file.header = header;
                Dictionary<short, HfzTile> tiles = readTiles(header, reader);
                file.mapData = tiles;
            }
            catch(Exception ex) 
            {
                throw ex;
            }
            finally
            {
                reader.Close();
                if (debug) 
                { 
                    writeLog.Close();
                }
            }

            return file;
        }

        public void saveFile(string fileName, FileFormat format, HfzFile file)
        {
            HfzHeader fh = validateHeader(file);

            // initiate the writing procedure
            EndianBinaryWriter fileWriter = null;

            try
            {
                // open file for writing
                Stream stream = Util.openStream(fileName, format, FileMode.Create);
                fileWriter = Util.createEndianBinaryWriterForStream(stream);

                hfzWriteHeader(ref fileWriter, fh);

                // writing tiles
                List<short> keys = file.getTileKeys();
                foreach (short key in keys)
                {                    
                    hfzWriteTile(ref fileWriter, fh, file.getTileData(key));
                }

                fileWriter.Close();
            }
            catch(Exception ex)
            {
                throw new Exception("Error writing file " + fileName, ex);
            }
            finally
            {
                // close file
                if (fileWriter != null)
                {
                    fileWriter.Close();
                    if (format == FileFormat.COMPRESSED)
                    {
                        Util.compressFile(fileName);
                    }
                }
            }
        }

        private void hfzWriteTile(ref EndianBinaryWriter writer, HfzHeader fh, HfzTile tile)
        {           
            List<float> pTileData = tile.tileData;

            Int32 i=0, TempInt, FirstVal;
            float f, HFmin, HFmax, VertScale, VertOffset;
            sbyte c;
            short s;        

            UInt32 nx = fh.nx;
            UInt32 ny = fh.ny;
            UInt32 TileSize = fh.TileSize;
            float Precis = fh.Precis;
            // get min/max alt in block (used for vert scale)
            HFmin = 0;
            HFmax = 0;
            Epsilon epsilon = new Epsilon(1E-3);

            for (i = 0; i < pTileData.Count; i++)
            {
                // find max diff in line

                f = pTileData.ElementAt(i);

                if (i == 0)
                {
                    HFmin = HFmax = f;
                }
                else
                {
                    if (RealExtensions.LT(f, HFmin, epsilon))
                    {
                        HFmin = f;
                    }

                    if (RealExtensions.GT(f, HFmax, epsilon))
                    {
                        HFmax = f;
                    }
                }                
            }

            // number of int levels required for this block
            float BlockLevels = ((HFmax - HFmin) / Precis) + 1;

            // calc scale 
            VertScale = (HFmax - HFmin) / BlockLevels;
            VertOffset = HFmin;
            if (RealExtensions.LE(VertScale, 0, epsilon))
            {
                VertScale = 1.0f; // this is for niceness
            }

            writer.Write(VertScale);
            writer.Write(VertOffset);   
                        
            // determine number of blocks
            Int32 nBlocksX = ((int)fh.nx / fh.TileSize) - 1;  // -1 due to index starting at 0
            Int32 exBlocksX = -1;
            Int32 nBlocksY = ((int)fh.ny / fh.TileSize) - 1;  // -1 due to index starting at 0
            Int32 exBlocksY = -1;
          
            if (fh.nx % fh.TileSize > 0)
            {
                exBlocksX = nBlocksX + 1;
            }

            if (fh.ny % fh.TileSize > 0)
            {
                exBlocksY = nBlocksY + 1;
            }

            UInt32 rows = TileSize;
            if(tile.x != exBlocksX && tile.y == exBlocksY || tile.x == exBlocksX && tile.y == exBlocksY)
            {
                rows = (uint)(TileSize - (((exBlocksY+1) * TileSize) - ny));
            }
            else if (tile.x == exBlocksX && tile.y != exBlocksY)
            {
                rows = calculateRows(tile, ny, TileSize, rows);
            }
            
            int rowCount = 0;
            int rowZeroElement = 0;
            try
            {
                for (rowCount = 0; rowCount < rows; rowCount++)
                {
                    List<float> row = null;

                    int range = calculateRange(tile, nx, TileSize);
                    
                    rowZeroElement = (int)(rowCount * range);
                    row = pTileData.GetRange(rowZeroElement, range);

                    f = row.ElementAt(0);
                    FirstVal = Convert.ToInt32(Math.Truncate((f - VertOffset) / VertScale));
                    Int32 LastVal = FirstVal;

                    // find max diff in line
                    Int32 Diff;
                    Int32 MaxDev = 0;
                    List<Int32> pDiffBuf = new List<Int32>();
                    for (int rowElementCounter = 1; rowElementCounter < row.Count; rowElementCounter++)
                    {

                        // find max diff in line
                        f = row.ElementAt(rowElementCounter);
                        TempInt = Convert.ToInt32(Math.Truncate((f - VertOffset) / VertScale));
                        Diff = TempInt - LastVal;

                        pDiffBuf.Add(Diff);
                        LastVal = TempInt;
                        MaxDev = MaxDev > Math.Abs(Diff) ? MaxDev : Math.Abs(Diff);
                    }

                    // should we use 8, 16 or 32 bit pixels?
                    byte LineDepth = 4;
                    if (MaxDev <= 127)
                    {
                        LineDepth = 1;
                    }
                    else
                        if (MaxDev <= 32767)
                        {
                            LineDepth = 2;
                        }

                    writer.Write(LineDepth);
                    writer.Write(FirstVal);

                    for (int rowElementCounter = 0; rowElementCounter < pDiffBuf.Count; rowElementCounter++)
                    {
                        Diff = pDiffBuf.ElementAt(rowElementCounter);
                        switch (LineDepth)
                        {
                            case 1:
                                c = Convert.ToSByte(Diff);
                                writer.Write(c);
                                break;
                            case 2:
                                s = Convert.ToInt16(Diff);
                                writer.Write(s);
                                break;
                            case 4:
                                writer.Write(Diff);
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("Tile x :" + tile.x);
                System.Console.WriteLine("Tile y :" + tile.y);
                System.Console.WriteLine("Rows :" + rows);
                System.Console.WriteLine("RowCount :" + rowCount);
                System.Console.WriteLine("RowZeroElement :" + rowZeroElement);
                System.Console.WriteLine("exBlocksX :" + exBlocksX);
                System.Console.WriteLine("exBlocksY :" + exBlocksY);
                System.Console.WriteLine("pTileData count :" + pTileData.Count);
                throw ex;
            }
        }

        private uint calculateRows(HfzTile tile, UInt32 ny, UInt32 TileSize, UInt32 rows)
        {
            rows = ny - (TileSize * tile.y);
            if (rows > TileSize)
            {
                rows = TileSize;
            }
            debugLine("rows: " + rows);
            return rows;
        }

        private int calculateRange(HfzTile tile, UInt32 nx, UInt32 TileSize)
        {
            int range = (int)(nx - (TileSize * tile.x));
            if (range > TileSize)
            {
                range = (int)TileSize;
            }
            debugLine("range: " + range);                   
            return range;
        }

        private void hfzWriteHeader(ref EndianBinaryWriter writer, HfzHeader fh)
        {
            // copy header into buffer
            writer.Write(Util.StringToByteArray("HF2", 4));
            writer.Write(fh.FileVersionNo);
            writer.Write(fh.nx);
            writer.Write(fh.ny);
            writer.Write(fh.TileSize);
            writer.Write(fh.Precis);
            writer.Write(fh.HorizScale);
            writer.Write(fh.countExtLength());

            // put extended header into a buffer
            hfzHeader_EncodeExtHeaderBuf(fh, ref writer);
        }

        private void hfzHeader_EncodeExtHeaderBuf(HfzHeader fh, ref EndianBinaryWriter writer)
        {
            if (fh.nExtHeaderBlocks == 0)
            {
                return;
            }

            if (fh.countExtLength() == 0)
            {
                // can't have zero length
                throw new Exception("Can't have a extended header of zero size");
            }

            HfzExtHeaderBlock pBlock;
            for (int i = 0; i < fh.nExtHeaderBlocks; i++)
            {
                pBlock = fh.pExtHeaderBlocks.ElementAt(i);

                writer.Write(Util.StringToByteArray(pBlock.BlockType, 4));
                writer.Write(Util.StringToByteArray(pBlock.BlockName, 16));
                writer.Write(pBlock.BlockLength);                

                if (pBlock.BlockLength > 0)
                {
                    writer.Write(pBlock.pBlockData);
                }
            }
        }

        private HfzHeader validateHeader(HfzFile file)
        {
            Epsilon epsilon = new Epsilon(1E-3);
            HfzHeader header = file.header;
            if(header != null)
            {
                float precis = header.Precis;
                ushort tileSize = header.TileSize;
                UInt32 nx = header.nx;
                UInt32 ny = header.ny;
                if(!RealExtensions.LE(precis, 0.0, epsilon))
                {
                    if (tileSize < 8 || tileSize > 65535)
                    {
                        throw new Exception("Invalid tilesize: " + tileSize);
                    }
                    else
                    {
                        if (nx <= 0 || ny <= 0)
                        {
                            throw new Exception("Invalid mapsize: " + nx + "," + ny);
                        }
                    }
                }
                else
                {
                    throw new Exception("Invalid precis: " + precis);
                }
            }
            else
            {
                throw new Exception("No header found in file");
            }
            return header;
        }

        private Dictionary<short, HfzTile> readTiles(HfzHeader header, EndianBinaryReader reader) 
        {
            // I know this is offensive but after all the headaches this gave me IT'S STAYING IN !
            Boolean extraTileBullshit = false;
            UInt32 nx = header.nx;
            UInt32 ny = header.ny;
       
            debugLine("nx " + nx);
            debugLine("ny " + ny);

            ushort TileSize = header.TileSize;

            // determine number of blocks
            UInt32 nBlocksX = nx / TileSize;
            UInt32 nBlocksY = ny / TileSize;
            if (nx % TileSize != 0)
            {
                nBlocksX++;
                extraTileBullshit = true;
            }

            if (ny % TileSize != 0)
            {
                nBlocksY++;
                extraTileBullshit = true;
            }

            debugLine("x: " + nBlocksX + ", y: " + nBlocksY + ", extraTileBullshit: " + extraTileBullshit);

            short nTile = 0;
            Dictionary<short, HfzTile> tiles = new Dictionary<short, HfzTile>();
            for (UInt32 yAxis = 0; yAxis < nBlocksY; yAxis++)
            {
                for (UInt32 xAxis = 0; xAxis < nBlocksX; xAxis++)
                {
                    tiles.Add(nTile, hfzReadTile(reader, header, xAxis, yAxis, extraTileBullshit));
                    nTile++;
                }
            }

            return tiles;
        }

        private HfzTile hfzReadTile(EndianBinaryReader reader, HfzHeader fh, UInt32 TileX, UInt32 TileY, Boolean extraPixelBullshit)
        {
            
            debugLine("TileX: "+ TileX +", TileY: " + TileY);         
            List<float> pTileData = new List<float>();
            UInt32 i = 0, j = 0;

            UInt32 TileSize = fh.TileSize;
            UInt32 mapWidth = fh.nx;
            UInt32 mapHeight = fh.ny;
            UInt32 Rows = TileSize;
            UInt32 RowSize = TileSize-1;

            if (extraPixelBullshit)
            {
                Epsilon e = new Epsilon(1E-3);
                if (RealExtensions.EQ(TileX, (mapWidth / TileSize), e))
                {
                    debugLine("Entering X on " + TileX + "," + TileY);
                    debugLine("result: " + (TileSize - (((TileX + 1) * TileSize) - mapWidth)));
                    Rows = TileSize;
                    RowSize = (TileSize - (((TileX + 1) * TileSize) - mapWidth)) - 1;
                }

                if (RealExtensions.EQ(TileY, (mapHeight / TileSize), e)) 
                {
                    debugLine("Entering Y on " + TileX + "," + TileY);
                    debugLine("result: " + (TileSize - (((TileY + 1) * TileSize) - mapHeight)));
                    Rows =  TileSize - (((TileY + 1) * TileSize) - mapHeight);
                    RowSize = TileSize - 1;
                }

                if (RealExtensions.EQ(TileX, (mapWidth / TileSize), e) && RealExtensions.EQ(TileY, (mapHeight / TileSize), e))
                {
                    debugLine("Entering X,Y on " + TileX + "," + TileY);
                    debugLine("result: " + (TileSize - (((TileY + 1) * TileSize) - mapHeight)));
                    debugLine("result: " + ((TileSize - (((TileX + 1) * TileSize) - mapWidth)) - 1));
                    Rows = TileSize - (((TileY + 1) * TileSize) - mapHeight);
                    RowSize = (TileSize - (((TileX + 1) * TileSize) - mapWidth)) - 1;
                }
            }

            // read vert offset and sale
            byte LineDepth = 0;
            Int32 FirstVal = 0;
            int tileValues = 0;
            int firstValues = 0;
            try
            {
                float VertScale = reader.ReadSingle();
                float VertOffset = reader.ReadSingle();

                for (j = 0; j < Rows; j++)
                {
                    LineDepth = reader.ReadByte(); // 1, 2, or 4
                    debugLine("Linedepth " + LineDepth);
                    FirstVal = reader.ReadInt32();
                    debugLine("FirstVal " + FirstVal);
                    tileValues++;
                    firstValues++;
                    
                    float pixelValue = (float)FirstVal * VertScale + VertOffset;

                    // set first pixel
                    pTileData.Add(pixelValue);
                    debugLine("FirstValue: " + pixelValue);

                    Int32 LastVal = FirstVal;
                    Int32 li;

                    for (i = 0; i < RowSize; i++)
                    {
                        switch (LineDepth)
                        {
                            case 1:
                                sbyte bvalue = reader.ReadSByte();
                                li = (Int32)bvalue;
                                break;
                            case 2:
                                short svalue = reader.ReadInt16();
                                li = (Int32)svalue;
                                break;
                            default:
                                li = reader.ReadInt32();
                                break;
                        }

                        pixelValue = (float)(li + LastVal) * VertScale + VertOffset;
                        LastVal = li + LastVal;

                        pTileData.Add(pixelValue);
                        tileValues++;
                        debugLine("v: " + pixelValue);
                    }
                }
                debugLine("TileValues: " + tileValues);
                debugLine("FirstValues: " + firstValues);
            }
            catch (Exception)
            {
                System.Console.WriteLine("TileSize: " + TileSize);
                System.Console.WriteLine("x: " + TileX);
                System.Console.WriteLine("y: " + TileY);
                System.Console.WriteLine("j: " + j);
                System.Console.WriteLine("i: " + i);
                System.Console.WriteLine("LineDepth: " + LineDepth);
                System.Console.WriteLine("FirstVal: " + FirstVal);
                throw;
            }
            debugLine("total tilevalues " + tileValues);
            debugLine("total firstvalues " + firstValues);
            return new HfzTile(TileX, TileY, RowSize+1, Rows, pTileData);
        }

        private HfzHeader hfzReadHeader(EndianBinaryReader reader)
        {

            HfzHeader fh = new HfzHeader();

            string heading = UTF8Encoding.UTF8.GetString(reader.ReadBytes(4));
            if (heading.Equals("HF2"))
            {
                throw new Exception("Invalid file format heading HF2 not found");
            }

            ushort fileVersion = reader.ReadUInt16();
            fh.FileVersionNo = fileVersion;

            UInt32 nx = reader.ReadUInt32();
            fh.nx = nx;

            UInt32 ny = reader.ReadUInt32();
            fh.ny = ny;

            ushort tileSize = reader.ReadUInt16();
            fh.TileSize = tileSize;

            float Precis = reader.ReadSingle();
            fh.Precis = Precis;

            float HorizScale = reader.ReadSingle();
            fh.HorizScale = HorizScale;

            UInt32 ExtHeaderLength = reader.ReadUInt32();
            fh.ExtHeaderLength = ExtHeaderLength;

            if (ExtHeaderLength > 0)
            {
                fh = decodeExtHeaderBuf(fh, reader);
            }

            return fh;
        }

        private HfzHeader decodeExtHeaderBuf(HfzHeader fh, EndianBinaryReader reader)
        {

            if (fh.ExtHeaderLength == 0)
            {
                return fh;
            }

            fh.nExtHeaderBlocks = 0;
            fh.pExtHeaderBlocks = new List<HfzExtHeaderBlock>();

            // read once to determine number of blocks
            bool DoneFlag = false;
            int offset = 0;
            do
            {
                if (offset == fh.ExtHeaderLength)
                {
                    DoneFlag = true;
                }
                else if (fh.ExtHeaderLength - offset < 24)
                {
                    // not enough for a complete header
                    throw new Exception("Not enough bytes for a full extended header found.  Header size: " + fh.ExtHeaderLength);
                }
                else
                {
                    string blockType = UTF8Encoding.UTF8.GetString(reader.ReadBytes(4));
                    string blockName = UTF8Encoding.UTF8.GetString(reader.ReadBytes(16));
                    UInt32 blockLength = reader.ReadUInt32();
                    byte[] data = reader.ReadBytes((int)blockLength);

                    HfzExtHeaderBlock block = new HfzExtHeaderBlock(blockType, blockName, blockLength, data);
                    fh.pExtHeaderBlocks.Add(block);

                    fh.nExtHeaderBlocks = fh.nExtHeaderBlocks + 1;
                    offset = offset + 24 + (int)block.BlockLength;
                }
            } while (!DoneFlag);

            return fh;
        }
    }
}
