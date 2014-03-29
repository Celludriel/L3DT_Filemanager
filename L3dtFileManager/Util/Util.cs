using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using MiscUtil.Conversion;
using MiscUtil.IO;

namespace L3dtFileManager
{
    class Util
    {        
        public static byte[] StringToByteArray(string str, int length)
        {
            return Encoding.ASCII.GetBytes(str.PadRight(length, '\0'));
        }

        public static String toBitString(byte[] bArray)
        {
            StringBuilder str = new StringBuilder(8);
            for (int index = 0; index < bArray.Length; index++)
            {
                byte b = bArray[index];
                int[] bl = new int[8];

                for (int i = 0; i < bl.Length; i++)
                {
                    bl[bl.Length - 1 - i] = ((b & (1 << i)) != 0) ? 1 : 0;
                }

                foreach (int num in bl) str.Append(num);
            }

            return str.ToString();
        }

        public static EndianBitConverter getEndian() 
        {
            EndianBitConverter endian = EndianBitConverter.Big;
            if (BitConverter.IsLittleEndian)
            {
                endian = EndianBitConverter.Little;
            }
            return endian;
        }

        public static EndianBinaryReader createEndianBinaryReaderForStream(Stream stream)
        {
            EndianBitConverter endian = getEndian();
            EndianBinaryReader reader = new EndianBinaryReader(endian, stream);
            return reader;
        }

        public static EndianBinaryWriter createEndianBinaryWriterForStream(Stream stream)
        {
            EndianBitConverter endian = getEndian();
            EndianBinaryWriter writer = new EndianBinaryWriter(endian, stream);
            return writer;
        }

        public static Stream openStream(string lpFileName, FileFormat format, FileMode lpMode)
        {
            try
            {
                switch (format)
                {
                    case FileFormat.UNCOMPRESSED:
                        return new FileStream(@lpFileName, lpMode);
                    case FileFormat.COMPRESSED:
                        Stream fileStream = new FileStream(@lpFileName, lpMode);
                        if (lpMode == FileMode.Open)
                        {
                            return new GZipStream(fileStream, CompressionMode.Decompress);
                        }
                        else
                        {
                            return fileStream;
                        }
                    default:
                        throw new Exception("No valid file format given");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("File could not be opened", ex);
            }
        }

        public static void compressFile(string fileName)
        {
            Stream fileStream = new FileStream(@fileName, FileMode.Open);
            byte[] bytesToCompress = Util.getByteArrayFromStream(fileStream);
            fileStream.Close();
            fileStream = new FileStream(@fileName, FileMode.Create);
            GZipStream gzip = new GZipStream(fileStream, CompressionMode.Compress);
            gzip.Write(bytesToCompress, 0, bytesToCompress.Length);
            gzip.Close();
        }

        public static byte[] getByteArrayFromStream(Stream stream)
        {
            long originalPosition = 0;

            if (stream.CanSeek)
            {
                originalPosition = stream.Position;
                stream.Position = 0;
            }

            try
            {
                byte[] readBuffer = new byte[4096];

                int totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
                {
                    totalBytesRead += bytesRead;

                    if (totalBytesRead == readBuffer.Length)
                    {
                        int nextByte = stream.ReadByte();
                        if (nextByte != -1)
                        {
                            byte[] temp = new byte[readBuffer.Length * 2];
                            Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                            Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                            readBuffer = temp;
                            totalBytesRead++;
                        }
                    }
                }

                byte[] buffer = readBuffer;
                if (readBuffer.Length != totalBytesRead)
                {
                    buffer = new byte[totalBytesRead];
                    Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
                }
                return buffer;
            }
            finally
            {
                if (stream.CanSeek)
                {
                    stream.Position = originalPosition;
                }
            }
        }
    }
}