using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MiscUtil.IO;

namespace L3dtFileManager.Amf
{
    class AmfManager:AbstractManager
    {
        public AmfFile loadFile(string fileName, FileFormat format)
        {
            Stream fs = null;
            EndianBinaryReader reader = null;
            try
            {
                debugLine("File format: " + format + ", file name: " + fileName);
                fs = Util.openStream(fileName, format, FileMode.Open);
                reader = Util.createEndianBinaryReaderForStream(fs);

                AmfFile file = readAmfFile(reader);
                return file;
            }
            catch(Exception ex)
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

        public void saveFile(string fileName, FileFormat format, AmfFile file)
        {
            Stream fs = null;
            EndianBinaryWriter writer = null;
            try
            {
                debugLine("File format: " + format + ", file name: " + fileName);
                fs = Util.openStream(fileName, format, FileMode.Create);
                writer = Util.createEndianBinaryWriterForStream(fs);

                writeAmfFile(writer, file);
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
                    if (format == FileFormat.COMPRESSED)
                    {
                        Util.compressFile(fileName);
                    }
                }
            }
        }

        private void writeAmfFile(EndianBinaryWriter writer, AmfFile file)
        {
            writer.Write(Util.StringToByteArray("L3DT", 4));
            ushort fileType = 520;
            writer.Write(fileType);

            writer.Write(file.width);
            writer.Write(file.height);

            foreach (AmfPixelInfo pixel in file.pixels)
            {
                writer.Write(pixel.landTypeId);
                writer.Write(pixel.climateId);
            }
        }

        private AmfFile readAmfFile(EndianBinaryReader reader)
        {

            validateFile(reader);

            ushort width = BitConverter.ToUInt16(reader.ReadBytes(2), 0);
            debugLine("width: " + width);
            ushort height = BitConverter.ToUInt16(reader.ReadBytes(2), 0);
            debugLine("width: " + height);
            AmfFile file = new AmfFile(width, height);
            readPixels(reader, width, height, file);

            return file;
        }

        private void readPixels(EndianBinaryReader reader, ushort width, ushort height, AmfFile file)
        {
            int totalPixels = width * height;
            uint xLocation = 1;
            uint yLocation = 1;
            for (int i = 0; i < totalPixels; i++)
            {
                byte landTypeId = reader.ReadByte();
                byte climateId = reader.ReadByte();
                file.addPixel(new AmfPixelInfo(xLocation, yLocation, landTypeId, climateId));

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
            if (maptype != 520)
            {
                throw new Exception("Invalid file type maptype was not 520 but " + maptype);
            }
        }
    }
}
