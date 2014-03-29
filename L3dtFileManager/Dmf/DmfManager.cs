using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MiscUtil.IO;

namespace L3dtFileManager.Dmf
{
    public class DmfManager:AbstractManager
    {
        public DmfFile loadFile(string fileName)
        {
            Stream fs = null;
            EndianBinaryReader reader = null;
            try
            {
                debugLine("file name: " + fileName);
                fs = Util.openStream(fileName, FileFormat.UNCOMPRESSED, FileMode.Open);
                reader = Util.createEndianBinaryReaderForStream(fs);

                DmfFile file = readDmfFile(reader);
                return file;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
            }
        }

        public void saveFile(string fileName, DmfFile file)
        {
            Stream fs = null;
            EndianBinaryWriter writer = null;
            try
            {
                debugLine("file name: " + fileName);
                fs = Util.openStream(fileName, FileFormat.UNCOMPRESSED, FileMode.Create);
                writer = Util.createEndianBinaryWriterForStream(fs);

                writeDmfFile(writer, file);
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
            }
        }

        private void writeDmfFile(EndianBinaryWriter writer, DmfFile file)
        {
            writer.Write(Util.StringToByteArray("L3DT", 4));
            ushort fileType = 200;
            writer.Write(fileType);

            writer.Write(file.width);
            writer.Write(file.height);
            byte wrapFlag = file.wrapFlag == true ? (byte)1 : (byte)0;
            writer.Write(wrapFlag);

            foreach (DmfPixelInfo pixel in file.pixels)
            {
                writer.Write(pixel.altitude);
                writer.Write(pixel.peakRoughness);
                writer.Write(pixel.fractalRoughness);
                writer.Write(pixel.cliffStrength);
                writer.Write(pixel.erosionStrength);
                writer.Write(pixel.autoLakeStrength);
                writer.Write(pixel.climateId);
                writer.Write(pixel.specialTypeId);
                writer.Write(pixel.specialTypeParam);
            }
        }

        private DmfFile readDmfFile(EndianBinaryReader reader)
        {
            validateFile(reader);

            ushort width = BitConverter.ToUInt16(reader.ReadBytes(2), 0);
            debugLine("width: " + width);
            ushort height = BitConverter.ToUInt16(reader.ReadBytes(2), 0);
            debugLine("width: " + height);
            byte wrapFlag = reader.ReadByte();
            DmfFile file = new DmfFile(width, height, wrapFlag == 1 ? true : false);
            readPixels(reader, width, height, file);
            return file;
        }

        private void readPixels(EndianBinaryReader reader, ushort width, ushort height, DmfFile file)
        {
            int totalPixels = width * height;
            uint xLocation = 1;
            uint yLocation = 1;
            for (int i = 0; i < totalPixels; i++)
            {
                short altitude = reader.ReadInt16();
                byte peakRoughness = reader.ReadByte();
                byte fractalRoughness = reader.ReadByte();
                byte cliffStrength = reader.ReadByte();
                byte erosionStrength = reader.ReadByte();
                byte autoLakeStrength = reader.ReadByte();
                byte climateId = reader.ReadByte();
                byte specialTypeId = reader.ReadByte();
                byte specialTypeParam = reader.ReadByte();

                file.addPixel(new DmfPixelInfo(xLocation, yLocation, altitude, peakRoughness, fractalRoughness, cliffStrength, erosionStrength, autoLakeStrength, climateId, specialTypeId, specialTypeParam));

                xLocation++;
                if (xLocation > width)
                {
                    xLocation = 1;
                    yLocation++;
                }
            }

            debugLine("pixel count: " + file.pixels.Count());
        }

        private void validateFile(EndianBinaryReader reader)
        {
            string marker = UTF8Encoding.UTF8.GetString(reader.ReadBytes(4));
            if (!"L3DT".Equals(marker))
            {
                throw new Exception("Invalid file type marker was not L3DT");
            }

            ushort maptype = BitConverter.ToUInt16(reader.ReadBytes(2), 0);
            if (maptype != 200)
            {
                throw new Exception("Invalid file type maptype was not 200 but " + maptype);
            }
        }
    }
}
