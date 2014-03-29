using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace L3dtFileManager.Wmf
{
    public class WmfFile
    {
        public WmfHeader header;
        public List<WmfPixelInfo> pixels = new List<WmfPixelInfo>();

        public void addPixel(WmfPixelInfo pixel)
        {
            this.pixels.Add(pixel);
        }

        public WmfPixelInfo getPixelAt(uint x, uint y)
        {
            foreach (WmfPixelInfo pixel in pixels)
            {
                if (pixel.x == x && pixel.y == y)
                {
                    return pixel;
                }
            }
            throw new Exception("No pixel found at x " + x + " and y " + y);
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("header\n");
            builder.Append(this.header);
            builder.Append("\ndatacount\n");
            builder.Append(this.pixels.Count());
            return builder.ToString();
        }
    }
}
