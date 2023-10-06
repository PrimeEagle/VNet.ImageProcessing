using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
#pragma warning disable CA1416

namespace VNet.ImageProcessing
{
    public class Geometry
    {
        public static Bitmap FlipHorizontal(Bitmap source)
        {
            var returnBitmap = new Bitmap(source.Width, source.Height);

            var sourceData = source.LockBits(new Rectangle(0, 0, source.Width, source.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            var destData = returnBitmap.LockBits(new Rectangle(0, 0, returnBitmap.Width, returnBitmap.Height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            var stride = sourceData.Stride;

            unsafe
            {
                var sourcePtr = (byte*)sourceData.Scan0;
                var destPtr = (byte*)destData.Scan0;

                for (var y = 0; y < source.Height; y++)
                {
                    for (var x = 0; x < source.Width; x++)
                    {
                        var destX = source.Width - x - 1;

                        var sourcePixel = sourcePtr + y * stride + x * 3;
                        var destPixel = destPtr + y * stride + destX * 3;

                        destPixel[0] = sourcePixel[0];
                        destPixel[1] = sourcePixel[1];
                        destPixel[2] = sourcePixel[2];
                    }
                }
            }

            source.UnlockBits(sourceData);
            returnBitmap.UnlockBits(destData);

            return returnBitmap;
        }

        public static Bitmap FlipVertical(Bitmap source)
        {
            var returnBitmap = new Bitmap(source.Width, source.Height);

            var sourceData = source.LockBits(new Rectangle(0, 0, source.Width, source.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            var destData = returnBitmap.LockBits(new Rectangle(0, 0, returnBitmap.Width, returnBitmap.Height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            var stride = sourceData.Stride;

            unsafe
            {
                var sourcePtr = (byte*)sourceData.Scan0;
                var destPtr = (byte*)destData.Scan0;

                for (var y = 0; y < source.Height; y++)
                {
                    var destY = source.Height - y - 1;

                    for (var x = 0; x < source.Width; x++)
                    {
                        var sourcePixel = sourcePtr + y * stride + x * 3;
                        var destPixel = destPtr + destY * stride + x * 3;

                        destPixel[0] = sourcePixel[0];
                        destPixel[1] = sourcePixel[1];
                        destPixel[2] = sourcePixel[2];
                    }
                }
            }

            source.UnlockBits(sourceData);
            returnBitmap.UnlockBits(destData);

            return returnBitmap;
        }

        public static Bitmap Rotate90Degrees(Bitmap source)
        {
            var returnBitmap = new Bitmap(source.Height, source.Width);

            var sourceData = source.LockBits(new Rectangle(0, 0, source.Width, source.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            var destData = returnBitmap.LockBits(new Rectangle(0, 0, returnBitmap.Width, returnBitmap.Height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            var strideSource = sourceData.Stride;
            var strideDest = destData.Stride;

            unsafe
            {
                var sourcePtr = (byte*)sourceData.Scan0;
                var destPtr = (byte*)destData.Scan0;

                for (var x = 0; x < source.Width; x++)
                {
                    for (var y = 0; y < source.Height; y++)
                    {
                        var destY = source.Width - x - 1;

                        var sourcePixel = sourcePtr + y * strideSource + x * 3;
                        var destPixel = destPtr + destY * strideDest + y * 3;

                        destPixel[0] = sourcePixel[0]; // Blue
                        destPixel[1] = sourcePixel[1]; // Green
                        destPixel[2] = sourcePixel[2]; // Red
                    }
                }
            }

            source.UnlockBits(sourceData);
            returnBitmap.UnlockBits(destData);

            return returnBitmap;
        }

        public static Bitmap Scale(Bitmap source, double scaleFactor)
        {
            var newWidth = (int)(source.Width * scaleFactor);
            var newHeight = (int)(source.Height * scaleFactor);

            var result = new Bitmap(newWidth, newHeight);

            var sourceData = source.LockBits(new Rectangle(0, 0, source.Width, source.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            var resultData = result.LockBits(new Rectangle(0, 0, newWidth, newHeight), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            var bytesPerPixel = Image.GetPixelFormatSize(source.PixelFormat) / 8;
            var pixelBuffer = new byte[bytesPerPixel];

            for (var y = 0; y < newHeight; y++)
            {
                for (var x = 0; x < newWidth; x++)
                {
                    var srcX = (int)(x / scaleFactor);
                    var srcY = (int)(y / scaleFactor);

                    var sourcePixel = sourceData.Scan0 + srcY * sourceData.Stride + srcX * bytesPerPixel;
                    Marshal.Copy(sourcePixel, pixelBuffer, 0, bytesPerPixel);

                    var destPixel = resultData.Scan0 + y * resultData.Stride + x * bytesPerPixel;
                    Marshal.Copy(pixelBuffer, 0, destPixel, bytesPerPixel);
                }
            }

            source.UnlockBits(sourceData);
            result.UnlockBits(resultData);

            return result;
        }


        public static Bitmap Crop(Bitmap source, Rectangle cropRectangle)
        {
            var result = new Bitmap(cropRectangle.Width, cropRectangle.Height);

            var sourceData = source.LockBits(cropRectangle, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            var resultData = result.LockBits(new Rectangle(0, 0, cropRectangle.Width, cropRectangle.Height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            var sourcePosition = sourceData.Scan0;
            var destPosition = resultData.Scan0;

            var bytesPerPixel = Image.GetPixelFormatSize(source.PixelFormat) / 8;
            var sourceStride = sourceData.Stride;
            var destStride = resultData.Stride;

            for (var y = 0; y < cropRectangle.Height; y++)
            {
                var rowData = new byte[cropRectangle.Width * bytesPerPixel];
                Marshal.Copy(sourcePosition + y * sourceStride, rowData, 0, cropRectangle.Width * bytesPerPixel);
                Marshal.Copy(rowData, 0, destPosition + y * destStride, cropRectangle.Width * bytesPerPixel);
            }

            source.UnlockBits(sourceData);
            result.UnlockBits(resultData);

            return result;
        }
    }
}