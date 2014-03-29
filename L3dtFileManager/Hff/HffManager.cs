using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MiscUtil.IO;

namespace L3dtFileManager.Hff
{
    public class HffManager : AbstractManager
    {
        public HffFile loadFile(string fileName)
        {
            HffFile file = new HffFile();

            Stream fs = null;
            EndianBinaryReader reader = null;
            try
            {
                debugLine("file name: " + fileName);
                //create a file stream to work with during load
                fs = Util.openStream(fileName, FileFormat.UNCOMPRESSED, FileMode.Open);
                reader = Util.createEndianBinaryReaderForStream(fs);

                HffHeader header = readHffHeader(reader);
                file.header = header;
                if (header.tileSize <= 1)
                {
                    readConventionalData(reader, header, file);
                }
                else
                {
                    readTiledData(reader, header, file);
                }
            }
            catch (Exception ex)
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
                setDebug(false, null);
            }

            return file;
        }

        public void saveFile(string fileName, HffFile file)
        {
            Stream fs = null;
            EndianBinaryWriter writer = null;
            try
            {
                debugLine("file name: " + fileName);
                fs = Util.openStream(fileName, FileFormat.UNCOMPRESSED, FileMode.Create);
                writer = Util.createEndianBinaryWriterForStream(fs);

                writeHffHeader(writer, file);
                writeHffData(writer, file);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (writer != null)
                {
                    writer.Close();
                }
                setDebug(false, null);
            }
        }

        private void writeHffHeader(EndianBinaryWriter writer, HffFile file)
        {
            HffHeader header = file.header;
            writer.Write(Util.StringToByteArray("L3DT", 4));
            ushort fileType = 300;
            writer.Write(fileType);
            writer.Write(Util.StringToByteArray("HFF_v1.0", 8));
            writer.Write(header.dataOffset);
            writer.Write(header.width);
            writer.Write(header.height);
            writer.Write(header.dataSize);
            writer.Write(header.floatingPointFlag == true ? (byte)1 : (byte)0);
            writer.Write(header.verticalScale);
            writer.Write(header.verticalOffset);
            writer.Write(header.horizontalScale);
            writer.Write(header.tileSize);
            writer.Write(header.wrapFlag == true ? (byte)1 : (byte)0);
            writer.Write(header.reserved);
        }

        private void writeHffData(EndianBinaryWriter writer, HffFile file)
        {
            float minalt = findMinAltitude(file.pixels);
            debugLine("minalt: " + minalt);
            float maxalt = findMaxAltitude(file.pixels);
            debugLine("maxalt: " + maxalt);
            float vertScale, formulaResult;
            foreach (HffPixelInfo pixel in file.pixels)
            {
                if (file.header.floatingPointFlag)
                {
                    formulaResult = ((pixel.data - 0) / 1);
                    debugLine("v: " + formulaResult);
                    writer.Write(formulaResult);
                }
                else
                {
                    switch (file.header.dataSize)
                    {
                        case 1:
                            vertScale = (maxalt - minalt) / 255;
                            debugLine("vertScale: " + vertScale);
                            formulaResult = (pixel.data - minalt) / vertScale;
                            debugLine("v: " + formulaResult);
                            writer.Write(Convert.ToSByte(formulaResult));
                            break;
                        case 2: 
                            vertScale = (maxalt - minalt) / 65535;
                            debugLine("vertScale: " + vertScale);
                            formulaResult = (pixel.data - minalt) / vertScale;
                            debugLine("v: " + formulaResult);
                            writer.Write(Convert.ToUInt16(formulaResult));
                            break;
                        default: throw new Exception("Wrong datasize for pixel at x: " + pixel.x + ", y: " + pixel.y);
                    }
                }
            }
        }

        private float findMaxAltitude(List<HffPixelInfo> list)
        {
            float max = 0.0f;
            foreach (HffPixelInfo pixel in list)
            {
                if (pixel.data > max)
                {
                    max = pixel.data;
                }
            }
            return max;
        }

        private float findMinAltitude(List<HffPixelInfo> list)
        {
            float min = 0.0f;
            foreach (HffPixelInfo pixel in list)
            {
                if(pixel.data < min)
                {
                    min = pixel.data;
                }
            }
            return min;
        }

        private void readTiledData(EndianBinaryReader reader, HffHeader header, HffFile file)
        {
            validateTileSizeForDimensions(header);

            uint horizontalTiles = header.width / header.tileSize;
            uint verticalTiles = header.height / header.tileSize;

            for (int y = 0; y < verticalTiles; y++)
            {
                for (int x = 0; x < horizontalTiles; x++)
                {
                    readTile(x, y, header, reader, file);
                }
            }
        }

        private void readTile(int x, int y, HffHeader header, EndianBinaryReader reader, HffFile file)
        {
            for (int j = 1; j <= header.tileSize; j++) 
            {
                for (int i = 1; i <= header.tileSize; i++)
                {
                    uint xLocation = (uint)((x * header.tileSize) + i);
                    uint yLocation = (uint)((y * header.tileSize) + j);
                    float value = readValueFromReader(reader, header, xLocation, yLocation);
                    file.addPixel(new HffPixelInfo(xLocation, yLocation, value));
                }
            }
        }

        private void validateTileSizeForDimensions(HffHeader header)
        {
            if (header.width % header.tileSize != 0 || header.height % header.tileSize != 0)
            {
                throw new Exception("Map dimensions do not fit the tilesize");
            }
        }

        private void readConventionalData(EndianBinaryReader reader, HffHeader header, HffFile file)
        {
            uint totalPixels = header.width * header.height;
            uint xLocation = 1;
            uint yLocation = 1;
            for (int i = 0; i < totalPixels; i++)
            {
                float value = readValueFromReader(reader, header, xLocation, yLocation);
                file.addPixel(new HffPixelInfo(xLocation, yLocation, value));

                xLocation++;
                if (xLocation > header.width)
                {
                    xLocation = 1;
                    yLocation++;
                }
            }

            debugLine("pixel count: " + file.pixels.Count());
        }

        private float readValueFromReader(EndianBinaryReader reader, HffHeader header, uint xLocation, uint yLocation)
        {
            float value;

            if (header.floatingPointFlag)
            {
                float readSingleValue = reader.ReadSingle();
                debugLine("v: " + readSingleValue);
                value = header.verticalScale * readSingleValue + header.verticalOffset;
                debugLine("cv: " + value);
            }
            else
            {
                switch (header.dataSize)
                {
                    case 1:
                        sbyte readSbyteValue = reader.ReadSByte();
                        debugLine("v: " + readSbyteValue);
                        value = header.verticalScale * readSbyteValue + header.verticalOffset;
                        debugLine("cv: " + value);
                        break;
                    case 2:
                        ushort readShortValue = reader.ReadUInt16();
                        debugLine("v: " + readShortValue);
                        value = header.verticalScale * readShortValue + header.verticalOffset;
                        debugLine("cv: " + value);
                        break;
                    default: throw new Exception("Invalid pixel data at x: " + xLocation + ", y: " + yLocation);
                }
            }
            return value;
        }

        private HffHeader readHffHeader(EndianBinaryReader reader)
        {
            validateFile(reader);

            ushort dataOffset = reader.ReadUInt16();
            uint width = reader.ReadUInt32();
            uint height = reader.ReadUInt32();
            byte dataSize = reader.ReadByte();
            bool floatingPointFlag = reader.ReadByte() == 1 ? true : false;
            float verticalScale = reader.ReadSingle();
            float verticalOffset = reader.ReadSingle();
            float horizontalScale = reader.ReadSingle();
            ushort tileSize = reader.ReadUInt16();
            bool wrapFlag = reader.ReadByte() == 1 ? true : false;
            byte[] reserved = reader.ReadBytes(dataOffset-41);

            return new HffHeader(dataOffset, width, height, dataSize, floatingPointFlag, verticalScale,
                verticalOffset, horizontalScale, tileSize, wrapFlag, reserved);
        }

        private void validateFile(EndianBinaryReader reader)
        {            
            string buffer = UTF8Encoding.UTF8.GetString(reader.ReadBytes(4));
            if (!"L3DT".Equals(buffer))
            {
                throw new Exception("Invalid file type marker was not L3DT");
            }

            ushort maptype = BitConverter.ToUInt16(reader.ReadBytes(2), 0);
            if (maptype != 300)
            {
                throw new Exception("Invalid file type maptype was not 300 but " + maptype);
            }

            buffer = UTF8Encoding.UTF8.GetString(reader.ReadBytes(8));
            if (!"HFF_v1.0".Equals(buffer))
            {
                throw new Exception("Invalid map type marker was not HFF_v1.0");
            }
        }
    }
}
