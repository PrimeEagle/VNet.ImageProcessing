using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
#pragma warning disable CA1416

namespace VNet.ImageProcessing
{
    public static class ColorProcessing
    {
        public static Bitmap ConvertToGrayscale(Bitmap sourceBitmap, double redWeight, double greenWeight, double blueWeight)
        {
            if (sourceBitmap == null)
                throw new ArgumentNullException(nameof(sourceBitmap));

            var width = sourceBitmap.Width;
            var height = sourceBitmap.Height;

            var grayscaleBitmap = new Bitmap(width, height);

            // Lock the source and destination bitmaps into memory
            var sourceData = sourceBitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            var destinationData = grayscaleBitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);

            unsafe
            {
                var sourcePointer = (byte*)sourceData.Scan0.ToPointer();
                var destinationPointer = (byte*)destinationData.Scan0.ToPointer();

                var sourceStride = sourceData.Stride;
                var destinationStride = destinationData.Stride;

                for (var y = 0; y < height; y++)
                {
                    for (var x = 0; x < width; x++)
                    {
                        // Get the color components
                        var blue = sourcePointer[x * 3];
                        var green = sourcePointer[x * 3 + 1];
                        var red = sourcePointer[x * 3 + 2];

                        // Calculate the grayscale value using the specified weights
                        var grayscaleValue = (byte)(red * redWeight + green * greenWeight + blue * blueWeight);

                        // Set the grayscale value in the destination bitmap
                        destinationPointer[x] = grayscaleValue;
                    }

                    // Move to the next row
                    sourcePointer += sourceStride;
                    destinationPointer += destinationStride;
                }
            }

            // Unlock the bitmaps
            sourceBitmap.UnlockBits(sourceData);
            grayscaleBitmap.UnlockBits(destinationData);

            // Create a grayscale palette
            var grayscalePalette = grayscaleBitmap.Palette;
            for (var i = 0; i < 256; i++)
            {
                grayscalePalette.Entries[i] = System.Drawing.Color.FromArgb(i, i, i);
            }
            grayscaleBitmap.Palette = grayscalePalette;

            return grayscaleBitmap;
        }

        public static Bitmap ColorCorrection(Bitmap source, double redMultiplier, double greenMultiplier, double blueMultiplier)
        {
            var result = new Bitmap(source.Width, source.Height);

            var sourceData = source.LockBits(new Rectangle(0, 0, source.Width, source.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            var resultData = result.LockBits(new Rectangle(0, 0, source.Width, source.Height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            var bytesPerPixel = Image.GetPixelFormatSize(source.PixelFormat) / 8;
            var pixelBuffer = new byte[sourceData.Stride * source.Height];

            Marshal.Copy(sourceData.Scan0, pixelBuffer, 0, pixelBuffer.Length);

            for (var i = 0; i < pixelBuffer.Length; i += bytesPerPixel)
            {
                pixelBuffer[i] = ImageUtil.Clamp((int)(pixelBuffer[i] * blueMultiplier));
                pixelBuffer[i + 1] = ImageUtil.Clamp((int)(pixelBuffer[i + 1] * greenMultiplier));
                pixelBuffer[i + 2] = ImageUtil.Clamp((int)(pixelBuffer[i + 2] * redMultiplier));
            }

            Marshal.Copy(pixelBuffer, 0, resultData.Scan0, pixelBuffer.Length);
            source.UnlockBits(sourceData);
            result.UnlockBits(resultData);

            return result;
        }
    }
}