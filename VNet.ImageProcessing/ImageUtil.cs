using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace VNet.ImageProcessing
{
    public static class ImageUtil
    {
        public static byte Clamp(int color)
        {
            return (byte)Math.Min(Math.Max(color, 0), 255);
        }

        public static byte Clamp(double color)
        {
            return (byte)Math.Min(Math.Max(color, 0), 255);
        }

        public static double[] BitmapToDoubleArray(Bitmap bitmap, bool normalize = false)
        {
            var width = bitmap.Width;
            var height = bitmap.Height;
            var format = bitmap.PixelFormat;
            var bytesPerPixel = System.Drawing.Image.GetPixelFormatSize(format) / 8;
            var totalBytes = width * height * bytesPerPixel;
            var arr = new double[width * height * bytesPerPixel];

            var bmpData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, format);
            var pixelData = new byte[totalBytes];
            Marshal.Copy(bmpData.Scan0, pixelData, 0, totalBytes);
            bitmap.UnlockBits(bmpData);

            var counter = 0;
            for (var i = 0; i < pixelData.Length; i += bytesPerPixel)
            {
                for (var j = 0; j < bytesPerPixel; j++)
                {
                    arr[counter++] = normalize ? pixelData[i + j] / 255.0 : pixelData[i + j];
                }
            }

            return arr;
        }

        public static Bitmap DoubleArrayToBitmap(double[] arr, int width, int height, PixelFormat format, bool denormalize = false)
        {
            var bytesPerPixel = System.Drawing.Image.GetPixelFormatSize(format) / 8;

            if (arr.Length != width * height * bytesPerPixel)
                throw new ArgumentException("Array size does not match bitmap dimensions.");

            var bitmap = new Bitmap(width, height, format);
            var pixelData = new byte[arr.Length];

            var counter = 0;
            for (var i = 0; i < pixelData.Length; i += bytesPerPixel)
            {
                for (var j = 0; j < bytesPerPixel; j++)
                {
                    pixelData[i + j] = denormalize ? (byte)(arr[counter++] * 255) : (byte)arr[counter++];
                }
            }

            var bmpData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, format);
            Marshal.Copy(pixelData, 0, bmpData.Scan0, pixelData.Length);
            bitmap.UnlockBits(bmpData);

            return bitmap;
        }
    }
}