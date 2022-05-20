using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NullEngine.Rendering.DataStructures
{
    public class ColorMap

    {/*
        public int xres { get; set; }
        public int yres { get; set; }

        public Bitmap GetDepthBitmap()
        {
            double min = double.MaxValue;
            double max = double.MinValue;
            for (int x = 0; x < this.xres; x++)
            {
                for (int y = 0; y < this.yres; y++)
                {
                    var val = this.DepthMap[x][y];
                    min = Math.Min(min, val);
                    max = Math.Max(max, val);
                }
            }


            var bitmap = new Bitmap(this.xres, this.yres);

            for (int x = 0; x < this.xres; x++)
            {
                for (int y = 0; y < this.yres; y++)
                {
                    var val = this.DepthMap[x][y];
                    var remap = ColorGenerator.Remap(val, min, max, 0, 1);
                    var pixColor = ColorGenerator.Turbo.ReturnTurboColor(remap);

                    bitmap.SetPixel(this.xres - x - 1, this.yres - y - 1, pixColor);

                }
            }

            return bitmap;
        }

        public Bitmap GetLabelBitmap()
        {

            double min = 0;
            double max = Enum.GetNames(typeof(SmoFace.SmoFaceType)).Length;

            var bitmap = new Bitmap(this.xres, this.yres);

            for (int x = 0; x < this.xres; x++)
            {
                for (int y = 0; y < this.yres; y++)
                {

                    double val = (double)((int)this.LabelMap[x][y]);
                    var remap = ColorGenerator.Remap(val, min, max, 0, 1);
                    var pixColor = ColorGenerator.Inferno.ReturnInfernoColor(remap);

                    bitmap.SetPixel(this.xres - x - 1, this.yres - y - 1, pixColor);

                }
            }

            return bitmap;
        }*/

    }
}
