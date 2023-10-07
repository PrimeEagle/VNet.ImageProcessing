using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
#pragma warning disable CA1416

namespace VNet.ImageProcessing
{
    public static class ImageUtil
    {
        public static byte Clamp(int color)
        {
            return (byte) Math.Min(Math.Max(color, 0), 255);
        }

        public static byte Clamp(double color)
        {
            return (byte) Math.Min(Math.Max(color, 0), 255);
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
                    pixelData[i + j] = denormalize ? (byte) (arr[counter++] * 255) : (byte) arr[counter++];
                }
            }

            var bmpData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, format);
            Marshal.Copy(pixelData, 0, bmpData.Scan0, pixelData.Length);
            bitmap.UnlockBits(bmpData);

            return bitmap;
        }

        public static Bitmap CreateBitmapFromKernel(byte[,] kernel)
        {
            var width = kernel.GetLength(0);
            var height = kernel.GetLength(1);

            var bitmap = new Bitmap(width, height, PixelFormat.Format8bppIndexed);

            // Lock the bitmap into memory
            var bitmapData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);

            try
            {
                unsafe
                {
                    var bitmapPointer = (byte*) bitmapData.Scan0.ToPointer();

                    for (var y = 0; y < height; y++)
                    {
                        for (var x = 0; x < width; x++)
                        {
                            // Set the pixel value based on the kernel values
                            var value = kernel[x, y];
                            bitmapPointer[y * bitmapData.Stride + x] = value;
                        }
                    }
                }
            }
            finally
            {
                // Unlock the bitmap
                bitmap.UnlockBits(bitmapData);
            }

            return bitmap;
        }

        public static Bitmap ConvertDensityMapToBitmap(double[,,] densityMap)
        {
            var width = densityMap.GetLength(0);
            var height = densityMap.GetLength(1);

            var bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);

            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    var pixelValue = (int)(densityMap[x, y, 0] * 255); // Assuming densityMap has one channel
                    var color = Color.FromArgb(pixelValue, pixelValue, pixelValue);
                    bitmap.SetPixel(x, y, color);
                }
            }

            return bitmap;
        }
    }
}