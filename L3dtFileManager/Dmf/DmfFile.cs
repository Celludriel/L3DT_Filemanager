using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace L3dtFileManager.Dmf
{
    public class DmfFile
    {
        public ushort width;
        public ushort height;
        public bool wrapFlag = false;
        public List<DmfPixelInfo> pixels = new List<DmfPixelInfo>();

        public DmfFile(ushort width, ushort height)
        {
            this.width = width;
            this.height = height;
        }

        public DmfFile(ushort width, ushort height, bool wrapFlag)
        {
            this.width = width;
            this.height = height;
            this.wrapFlag = wrapFlag;
        }

        public DmfFile(ushort width, ushort height, bool wrapFlag, List<DmfPixelInfo> pixels)
        {
            this.width = width;
            this.height = height;
            this.wrapFlag = wrapFlag;
            this.pixels = pixels;
        }

        public void addPixel(DmfPixelInfo pixel)
        {
            this.pixels.Add(pixel);
        }

        public DmfPixelInfo getPixelAt(uint x, uint y)
        {
            foreach (DmfPixelInfo pixel in pixels)
            {
                if (pixel.x == x && pixel.y == y)
                {
                    return pixel;
                }
            }
            throw new Exception("No pixel found at x " + x + " and y " + y);
        }
    }
}
