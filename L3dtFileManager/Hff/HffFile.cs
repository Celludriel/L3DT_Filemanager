using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace L3dtFileManager.Hff
{
    public class HffFile
    {
        public HffHeader header;
        public List<HffPixelInfo> pixels = new List<HffPixelInfo>();

        public void addPixel(HffPixelInfo pixel)
        {
            this.pixels.Add(pixel);
        }

        public HffPixelInfo getPixelAt(uint x, uint y)
        {
            foreach (HffPixelInfo pixel in pixels)
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
