using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MiscUtil.IO;

namespace L3dtFileManager.Wmf
{
    public class WmfManager:AbstractManager
    {
        public WmfFile loadFile(string fileName)
        {
            WmfFile file = new WmfFile();

            Stream fs = null;
            EndianBinaryReader reader = null;
            try
            {
                debugLine("file name: " + fileName);
                //create a file stream to work with during load
                fs = Util.openStream(fileName, FileFormat.UNCOMPRESSED, FileMode.Open);
                reader = Util.createEndianBinaryReaderForStream(fs);

                WmfHeader header = readWmfHeader(reader);
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

        public void saveFile(string fileName, WmfFile file)
        {
            Stream fs = null;
            EndianBinaryWriter writer = null;
            try
            {
                debugLine("file name: " + fileName);
                fs = Util.openStream(fileName, FileFormat.UNCOMPRESSED, FileMode.Create);
                writer = Util.createEndianBinaryWriterForStream(fs);

                writeWmfHeader(writer, file);
                writeWmfData(writer, file);
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

        private void writeWmfData(EndianBinaryWriter writer, WmfFile file)
        {
            float minalt = findMinAltitude(file.pixels);
            debugLine("minalt: " + minalt);
            float maxalt = findMaxAltitude(file.pixels);
            debugLine("maxalt: " + maxalt);
            float vertScale, formulaResult;
            foreach (WmfPixelInfo pixel in file.pixels)
            {
                if (file.header.floatingPointFlag)
                {
                    formulaResult = ((pixel.data - 0) / 1);
                    if (pixel.waterTypeId == 0)
                    {
                        formulaResult = (float)-1E10;
                    }
                    debugLine("v: " + formulaResult);
                    writer.Write(formulaResult);
                    writeWmfAuxPixelData(writer, file, pixel);
                }
                else
                {
                    switch (file.header.waterLevelDataSize)
                    {
                        case 2:
                            vertScale = (maxalt - minalt) / 65535;
                            debugLine("vertScale: " + vertScale);
                            formulaResult = (pixel.data - minalt) / vertScale;
                            if (pixel.waterTypeId == 0)
                            {
                                formulaResult = 0.0f;
                            }
                            else if (formulaResult < 0.0f)
                            {
                                formulaResult = 0.0f;
                            }
                            else if(formulaResult >  65535)
                            {
                                formulaResult = 65535.0f;
                            }

                            debugLine("v: " + formulaResult);
                            writer.Write(Convert.ToUInt16(formulaResult));
                            writeWmfAuxPixelData(writer, file, pixel);
                            break;
                        default: throw new Exception("Wrong datasize for pixel at x: " + pixel.x + ", y: " + pixel.y);
                    }
                }
            }

        }

        private static void writeWmfAuxPixelData(EndianBinaryWriter writer, WmfFile file, WmfPixelInfo pixel)
        {
            if (file.header.auxDataType > 0)
            {
                writer.Write(pixel.waterTypeId);
                writer.Write(pixel.waterBodyIndex);
            }
        }

        private float findMaxAltitude(List<WmfPixelInfo> list)
        {
            float max = 0.0f;
            foreach (WmfPixelInfo pixel in list)
            {
                if (pixel.data > max)
                {
                    max = pixel.data;
                }
            }
            return max;
        }

        private float findMinAltitude(List<WmfPixelInfo> list)
        {
            float min = 0.0f;
            foreach (WmfPixelInfo pixel in list)
            {
                if (pixel.data < min)
                {
                    min = pixel.data;
                }
            }
            return min;
        }

        private void writeWmfHeader(EndianBinaryWriter writer, WmfFile file)
        {
            WmfHeader header = file.header;
            writer.Write(Util.StringToByteArray("L3DT", 4));
            ushort fileType = 600;
            writer.Write(fileType);
            writer.Write(Util.StringToByteArray("WMF_v1.0", 8));
            writer.Write(header.dataOffset);
            writer.Write(header.width);
            writer.Write(header.height);
            writer.Write(header.waterLevelDataSize);
            writer.Write(header.floatingPointFlag == true ? (byte)1 : (byte)0);
            writer.Write(header.verticalScale);
            writer.Write(header.verticalOffset);
            writer.Write(header.horizontalScale);
            writer.Write(header.tileSize);
            writer.Write(header.wrapFlag == true ? (byte)1 : (byte)0);
            writer.Write((byte)0);
            writer.Write(header.auxDataType);
            writer.Write(header.auxDataSize);
            writer.Write(header.auxReserved);
        }

        private void readTiledData(EndianBinaryReader reader, WmfHeader header, WmfFile file)
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

        private void readTile(int x, int y, WmfHeader header, EndianBinaryReader reader, WmfFile file)
        {
            for (int j = 1; j <= header.tileSize; j++)
            {
                for (int i = 1; i <= header.tileSize; i++)
                {
                    uint xLocation = (uint)((x * header.tileSize) + i);
                    uint yLocation = (uint)((y * header.tileSize) + j);
                    readWmfValue(reader, header, file, xLocation, yLocation);
                }
            }
        }

        private void validateTileSizeForDimensions(WmfHeader header)
        {
            if (header.width % header.tileSize != 0 || header.height % header.tileSize != 0)
            {
                throw new Exception("Map dimensions do not fit the tilesize");
            }
        }

        private WmfHeader readWmfHeader(EndianBinaryReader reader)
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
            byte reserved = reader.ReadByte();

            if (reserved != 0)
            {
                throw new Exception("Reserved should be 0 for a WMF");
            }

            ushort auxDataType = reader.ReadUInt16();
            byte auxDataSize = reader.ReadByte();
            byte[] auxReserved = reader.ReadBytes(dataOffset - 45);

            return new WmfHeader(dataOffset, width, height, dataSize, floatingPointFlag, verticalScale,
                verticalOffset, horizontalScale, tileSize, wrapFlag, auxDataType, auxDataSize, auxReserved);
        }

        private void readConventionalData(EndianBinaryReader reader, WmfHeader header, WmfFile file)
        {
            uint totalPixels = header.width * header.height;
            uint xLocation = 1;
            uint yLocation = 1;
            for (int i = 0; i < totalPixels; i++)
            {
                readWmfValue(reader, header, file, xLocation, yLocation);
                xLocation++;
                if (xLocation > header.width)
                {
                    xLocation = 1;
                    yLocation++;
                }
            }

            debugLine("pixel count: " + file.pixels.Count());
        }

        private void readWmfValue(EndianBinaryReader reader, WmfHeader header, WmfFile file, uint xLocation, uint yLocation)
        {
            float value = readValueFromReader(reader, header, xLocation, yLocation);

            if (header.auxDataSize > 0)
            {
                byte waterTypeId = reader.ReadByte();
                ushort waterBodyIndex = reader.ReadUInt16();
                file.addPixel(new WmfPixelInfo(xLocation, yLocation, value, waterTypeId, waterBodyIndex));
            }
            else
            {
                file.addPixel(new WmfPixelInfo(xLocation, yLocation, value));
            }
        }

        private float readValueFromReader(EndianBinaryReader reader, WmfHeader header, uint xLocation, uint yLocation)
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
                switch (header.waterLevelDataSize)
                {
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

        private void validateFile(EndianBinaryReader reader)
        {
            string buffer = UTF8Encoding.UTF8.GetString(reader.ReadBytes(4));
            if (!"L3DT".Equals(buffer))
            {
                throw new Exception("Invalid file type marker was not L3DT");
            }

            ushort maptype = BitConverter.ToUInt16(reader.ReadBytes(2), 0);
            if (maptype != 600)
            {
                throw new Exception("Invalid file type maptype was not 600 but " + maptype);
            }

            buffer = UTF8Encoding.UTF8.GetString(reader.ReadBytes(8));
            if (!"WMF_v1.0".Equals(buffer))
            {
                throw new Exception("Invalid map type marker was not WMF_v1.0");
            }
        }
    }
}

