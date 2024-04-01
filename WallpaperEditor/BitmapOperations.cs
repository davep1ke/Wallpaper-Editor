using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;

namespace WallpaperEditor
{
    static class BitmapOperations
    {
        private static Int32Rect defaultArg = new Int32Rect(0, 0, 0, 0);

        public static WriteableBitmap createCanvas(int x, int y, BitmapSource bitmapTemplate)
        {
            return new WriteableBitmap(x, y, bitmapTemplate.DpiX, bitmapTemplate.DpiY, bitmapTemplate.Format, null);
        }

        public static WriteableBitmap createDummyBitmap()
        {
            return new WriteableBitmap(1, 1, 1, 1, PixelFormats.Bgr101010, BitmapPalettes.BlackAndWhite);
        }

        public static int get_stride(BitmapSource bitmapTemplate) {return bitmapTemplate.PixelWidth * (bitmapTemplate.Format.BitsPerPixel / 8); }

        public static Int32Rect getDims(BitmapSource source)
        {
            return new Int32Rect(0, 0, source.PixelWidth, source.PixelHeight);
        }

        public static WriteableBitmap copyArea(BitmapSource imageSource
            , WriteableBitmap newCanvas
            , Int32Rect? imageSourceDims = null
            , Int32Rect? newCanvasDims = null 
            , bool mirrorXPixels = false
            , bool mirrorYPixels = false
            )
        {

            if (imageSourceDims == null)
            {
                imageSourceDims = getDims(imageSource);
            }
            if (newCanvasDims == null)
            {
                newCanvasDims = getDims(newCanvas);
            }

            //if we have selected more than the size of our image, 

            //get a new bitmap with the target area scaled appropriately


            double xScale = ((double)newCanvasDims.Value.Width / (double)imageSourceDims.Value.Width);
            double yScale = ((double)newCanvasDims.Value.Height / (double)imageSourceDims.Value.Height);
            
            if (mirrorXPixels) { xScale = xScale * -1; }
            if (mirrorYPixels) { yScale = yScale * -1; }

            //cut out the pixels
            var cutout = new CroppedBitmap(imageSource, imageSourceDims.Value);

            //then scale to right size
            var scaled = new TransformedBitmap(cutout, new ScaleTransform(xScale, yScale));
            //at this point, scaled should be the same size as our newCanvasDims


            //copy sourcedata into temparray
            byte[] data = new byte[scaled.PixelHeight * get_stride(scaled)];
            scaled.CopyPixels(data, get_stride(scaled), 0);

            //copy into our output
            newCanvas.WritePixels(getDims(scaled), data, get_stride(scaled), newCanvasDims.Value.X, newCanvasDims.Value.Y );

            return newCanvas;


        }
    }
}
