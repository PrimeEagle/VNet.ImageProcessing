using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
#pragma warning disable CA1416

namespace VNet.ImageProcessing
{
    public class Enhancement
    {
        public static Bitmap GaussianBlur(Bitmap source, int kernelSize, double sigma)
        {
            if (kernelSize % 2 == 0) kernelSize++; // Ensure odd size

            // Create the 1D Gaussian kernel
            var kernel = new double[kernelSize];
            double kernelSum = 0;
            var foff = (kernelSize - 1) / 2;
            var factor = 1.0 / (Math.Sqrt(2.0 * Math.PI) * sigma);

            for (var x = -foff; x <= foff; x++)
            {
                kernel[x + foff] = factor * Math.Exp(-(x * x) / (2 * sigma * sigma));
                kernelSum += kernel[x + foff];
            }

            // Normalize the kernel
            for (var x = 0; x < kernelSize; x++)
            {
                kernel[x] /= kernelSum;
            }

            var temp = new Bitmap(source.Width, source.Height, PixelFormat.Format24bppRgb);
            var result = new Bitmap(source.Width, source.Height, PixelFormat.Format24bppRgb);

            var bytesPerPixel = Image.GetPixelFormatSize(source.PixelFormat) / 8;

            var sourceData = source.LockBits(new Rectangle(0, 0, source.Width, source.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            var tempData = temp.LockBits(new Rectangle(0, 0, temp.Width, temp.Height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            var resultData = result.LockBits(new Rectangle(0, 0, result.Width, result.Height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            unsafe
            {
                var sourcePtr = (byte*)sourceData.Scan0;
                var tempPtr = (byte*)tempData.Scan0;
                var resultPtr = (byte*)resultData.Scan0;

                // Horizontal pass
                for (var y = 0; y < source.Height; y++)
                {
                    for (var x = 0; x < source.Width; x++)
                    {
                        double red = 0.0, green = 0.0, blue = 0.0;
                        for (var k = -foff; k <= foff; k++)
                        {
                            var xPos = Math.Min(Math.Max(x + k, 0), source.Width - 1);
                            var kernelPixel = sourcePtr + y * sourceData.Stride + xPos * bytesPerPixel;

                            red += kernelPixel[2] * kernel[k + foff];
                            green += kernelPixel[1] * kernel[k + foff];
                            blue += kernelPixel[0] * kernel[k + foff];
                        }

                        var destPixel = tempPtr + y * tempData.Stride + x * bytesPerPixel;
                        destPixel[2] = (byte)red;
                        destPixel[1] = (byte)green;
                        destPixel[0] = (byte)blue;
                    }
                }

                // Vertical pass
                for (var y = 0; y < source.Height; y++)
                {
                    for (var x = 0; x < source.Width; x++)
                    {
                        double red = 0.0, green = 0.0, blue = 0.0;
                        for (var k = -foff; k <= foff; k++)
                        {
                            var yPos = Math.Min(Math.Max(y + k, 0), source.Height - 1);
                            var kernelPixel = tempPtr + yPos * tempData.Stride + x * bytesPerPixel;

                            red += kernelPixel[2] * kernel[k + foff];
                            green += kernelPixel[1] * kernel[k + foff];
                            blue += kernelPixel[0] * kernel[k + foff];
                        }

                        var destPixel = resultPtr + y * resultData.Stride + x * bytesPerPixel;
                        destPixel[2] = (byte)red;
                        destPixel[1] = (byte)green;
                        destPixel[0] = (byte)blue;
                    }
                }
            }

            source.UnlockBits(sourceData);
            temp.UnlockBits(tempData);
            result.UnlockBits(resultData);

            return result;
        }

        public static Bitmap Sharpen(Bitmap source, double strength)
        {
            // Create a sharpening kernel
            double[,] kernel = {
                                    { -strength, -strength, -strength },
                                    { -strength, 1 + 8 * strength, -strength },
                                    { -strength, -strength, -strength }
                                };

            var width = source.Width;
            var height = source.Height;
            var result = new Bitmap(width, height);

            var sourceData = source.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            var resultData = result.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            var bytesPerPixel = Image.GetPixelFormatSize(source.PixelFormat) / 8;
            var pixelBuffer = new byte[sourceData.Stride * height];
            var resultBuffer = new byte[sourceData.Stride * height];

            Marshal.Copy(sourceData.Scan0, pixelBuffer, 0, pixelBuffer.Length);

            const int filterOffset = 1;

            for (var offsetY = filterOffset; offsetY < height - filterOffset; offsetY++)
            {
                for (var offsetX = filterOffset; offsetX < width - filterOffset; offsetX++)
                {
                    double blue = 0;
                    double green = 0;
                    double red = 0;

                    var byteOffset = offsetY * sourceData.Stride + offsetX * bytesPerPixel;

                    for (var filterY = -filterOffset; filterY <= filterOffset; filterY++)
                    {
                        for (var filterX = -filterOffset; filterX <= filterOffset; filterX++)
                        {
                            var calcOffset = (offsetY + filterY) * sourceData.Stride + (offsetX + filterX) * bytesPerPixel;

                            blue += pixelBuffer[calcOffset] * kernel[filterY + filterOffset, filterX + filterOffset];
                            green += pixelBuffer[calcOffset + 1] * kernel[filterY + filterOffset, filterX + filterOffset];
                            red += pixelBuffer[calcOffset + 2] * kernel[filterY + filterOffset, filterX + filterOffset];
                        }
                    }

                    resultBuffer[byteOffset] = ImageUtil.Clamp(blue);
                    resultBuffer[byteOffset + 1] = ImageUtil.Clamp(green);
                    resultBuffer[byteOffset + 2] = ImageUtil.Clamp(red);
                }
            }

            Marshal.Copy(resultBuffer, 0, resultData.Scan0, resultBuffer.Length);
            source.UnlockBits(sourceData);
            result.UnlockBits(resultData);

            return result;
        }

        public static Bitmap AdjustBrightness(Bitmap source, int value)
        {
            var result = new Bitmap(source.Width, source.Height);

            var sourceData = source.LockBits(new Rectangle(0, 0, source.Width, source.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            var resultData = result.LockBits(new Rectangle(0, 0, source.Width, source.Height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            unsafe
            {
                var srcPointer = (byte*)sourceData.Scan0;
                var destPointer = (byte*)resultData.Scan0;

                for (var i = 0; i < sourceData.Stride * source.Height; i++)
                {
                    var newValue = srcPointer[i] + value;
                    destPointer[i] = (byte)Math.Min(Math.Max(newValue, 0), 255);
                }
            }

            source.UnlockBits(sourceData);
            result.UnlockBits(resultData);

            return result;
        }


        public static Bitmap AdjustContrast(Bitmap source, double contrastValue)
        {
            var result = new Bitmap(source.Width, source.Height);

            var sourceData = source.LockBits(new Rectangle(0, 0, source.Width, source.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            var resultData = result.LockBits(new Rectangle(0, 0, source.Width, source.Height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            unsafe
            {
                var srcPointer = (byte*)sourceData.Scan0;
                var destPointer = (byte*)resultData.Scan0;

                for (var i = 0; i < sourceData.Stride * source.Height; i++)
                {
                    var newValue = (srcPointer[i] - 128) * contrastValue + 128;
                    destPointer[i] = (byte)Math.Min(Math.Max(newValue, 0), 255);
                }
            }

            source.UnlockBits(sourceData);
            result.UnlockBits(resultData);

            return result;
        }


        public static Bitmap AdjustGamma(Bitmap source, double gammaValue)
        {
            var result = new Bitmap(source.Width, source.Height);

            var sourceData = source.LockBits(new Rectangle(0, 0, source.Width, source.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            var resultData = result.LockBits(new Rectangle(0, 0, source.Width, source.Height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            var gammaArray = new byte[256];

            for (var i = 0; i < 256; i++)
            {
                gammaArray[i] = (byte)Math.Min(Math.Max((int)(255.0 * Math.Pow(i / 255.0, 1.0 / gammaValue)), 0), 255);
            }

            unsafe
            {
                var srcPointer = (byte*)sourceData.Scan0;
                var destPointer = (byte*)resultData.Scan0;

                for (var i = 0; i < sourceData.Stride * source.Height; i++)
                {
                    destPointer[i] = gammaArray[srcPointer[i]];
                }
            }

            source.UnlockBits(sourceData);
            result.UnlockBits(resultData);

            return result;
        }

        public static Bitmap EdgeEnhancement(Bitmap source)
        {
            // Define a Laplacian kernel
            double[,] kernel = {
                                    { -1, -1, -1 },
                                    { -1,  8, -1 },
                                    { -1, -1, -1 }
                                };

            var width = source.Width;
            var height = source.Height;
            var result = new Bitmap(width, height);

            var sourceData = source.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            var resultData = result.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            const int filterOffset = 1;

            var stride = sourceData.Stride;
            var bytesPerPixel = Image.GetPixelFormatSize(source.PixelFormat) / 8;
            var pixelBuffer = new byte[stride * height];
            var resultBuffer = new byte[stride * height];

            Marshal.Copy(sourceData.Scan0, pixelBuffer, 0, pixelBuffer.Length);

            for (var offsetY = filterOffset; offsetY < height - filterOffset; offsetY++)
            {
                for (var offsetX = filterOffset; offsetX < width - filterOffset; offsetX++)
                {
                    double blue = 0;
                    double green = 0;
                    double red = 0;

                    var byteOffset = offsetY * stride + offsetX * bytesPerPixel;

                    for (var filterY = -filterOffset; filterY <= filterOffset; filterY++)
                    {
                        for (var filterX = -filterOffset; filterX <= filterOffset; filterX++)
                        {
                            var calcOffset = (offsetY + filterY) * stride + (offsetX + filterX) * bytesPerPixel;

                            blue += pixelBuffer[calcOffset] * kernel[filterY + filterOffset, filterX + filterOffset];
                            green += pixelBuffer[calcOffset + 1] * kernel[filterY + filterOffset, filterX + filterOffset];
                            red += pixelBuffer[calcOffset + 2] * kernel[filterY + filterOffset, filterX + filterOffset];
                        }
                    }

                    // Add the computed Laplacian value to the original image value to enhance the edges
                    resultBuffer[byteOffset] = ImageUtil.Clamp((int)(pixelBuffer[byteOffset] + blue));
                    resultBuffer[byteOffset + 1] = ImageUtil.Clamp((int)(pixelBuffer[byteOffset + 1] + green));
                    resultBuffer[byteOffset + 2] = ImageUtil.Clamp((int)(pixelBuffer[byteOffset + 2] + red));
                }
            }

            Marshal.Copy(resultBuffer, 0, resultData.Scan0, resultBuffer.Length);
            source.UnlockBits(sourceData);
            result.UnlockBits(resultData);

            return result;
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

        public static Bitmap Convolve(Bitmap source, double[,] kernel)
        {
            var width = source.Width;
            var height = source.Height;
            var result = new Bitmap(width, height);

            var kernelWidth = kernel.GetLength(0);
            var kernelHeight = kernel.GetLength(1);
            var kernelOffset = kernelWidth / 2;

            var sourceData = source.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            var resultData = result.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            unsafe
            {
                var srcPointer = (byte*)sourceData.Scan0;
                var destPointer = (byte*)resultData.Scan0;

                for (var y = kernelOffset; y < height - kernelOffset; y++)
                {
                    for (var x = kernelOffset; x < width - kernelOffset; x++)
                    {
                        double blue = 0.0, green = 0.0, red = 0.0;

                        for (var filterY = -kernelOffset; filterY <= kernelOffset; filterY++)
                        {
                            for (var filterX = -kernelOffset; filterX <= kernelOffset; filterX++)
                            {
                                var pixel = srcPointer + (y + filterY) * sourceData.Stride + (x + filterX) * 3;
                                blue += pixel[0] * kernel[filterY + kernelOffset, filterX + kernelOffset];
                                green += pixel[1] * kernel[filterY + kernelOffset, filterX + kernelOffset];
                                red += pixel[2] * kernel[filterY + kernelOffset, filterX + kernelOffset];
                            }
                        }

                        var destPixel = destPointer + y * sourceData.Stride + x * 3;
                        destPixel[0] = ImageUtil.Clamp(blue);
                        destPixel[1] = ImageUtil.Clamp(green);
                        destPixel[2] = ImageUtil.Clamp(red);
                    }
                }
            }

            source.UnlockBits(sourceData);
            result.UnlockBits(resultData);

            return result;
        }

        public static Bitmap Invert(Bitmap image)
        {
            var inverted = new Bitmap(image.Width, image.Height);

            for (var y = 0; y < image.Height; y++)
            {
                for (var x = 0; x < image.Width; x++)
                {
                    var pixel = image.GetPixel(x, y);
                    inverted.SetPixel(x, y, Color.FromArgb(255 - pixel.R, 255 - pixel.G, 255 - pixel.B));
                }
            }

            return inverted;
        }
    }
}