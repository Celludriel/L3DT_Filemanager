using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace L3dtFileManager.Amf
{
    public class AmfFile
    {
        public ushort width;
        public ushort height;
        public List<AmfPixelInfo> pixels = new List<AmfPixelInfo>();
        public Dictionary<byte, List<byte>> uniqueLandTypeIdProClimate = new Dictionary<byte, List<byte>>();

        public AmfFile(ushort width, ushort height)
        {
            this.width = width;
            this.height = height;
        }

        public AmfFile(ushort width, ushort height, List<AmfPixelInfo> pixels)
        {
            this.width = width;
            this.height = height;
            this.pixels = pixels;

            foreach (AmfPixelInfo pixel in pixels)
            {
                addLandTypeToDictionary(pixel);
            }
        }

        public void addPixel(AmfPixelInfo pixel) 
        {
            this.pixels.Add(pixel);
            addLandTypeToDictionary(pixel);
        }

        public AmfPixelInfo getPixelAt(uint x, uint y) 
        {
            foreach(AmfPixelInfo pixel in pixels)
            {
                if (pixel.x == x && pixel.y == y)
                {
                    return pixel;
                }
            }
            throw new Exception("No pixel found at x " + x + " and y " + y);
        }

        private void addLandTypeToDictionary(AmfPixelInfo pixel)
        {
            List<byte> landTypes = null;
            if (this.uniqueLandTypeIdProClimate.ContainsKey(pixel.climateId))
            {
                landTypes = this.uniqueLandTypeIdProClimate[pixel.climateId];
            }
            else
            {
                landTypes = new List<byte>();
            }

            if (!landTypes.Contains(pixel.landTypeId))
            {
                landTypes.Add(pixel.landTypeId);
            }
        
            this.uniqueLandTypeIdProClimate[pixel.climateId] = landTypes;               
        }

    }
}
