using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
// ReSharper disable MemberCanBePrivate.Global
#pragma warning disable CA1416

namespace VNet.ImageProcessing
{
    public static class Morphology
    {
        public static Bitmap Erode(Bitmap img)
        {
            return Morphology.PerformMorphologicalOperation(img, true);
        }

        public static Bitmap Dilate(Bitmap img)
        {
            return Morphology.PerformMorphologicalOperation(img, false);
        }

        private static Bitmap PerformMorphologicalOperation(Bitmap img, bool isErosion, byte threshold = 128, int kernelSize = 5)
        {
            var width = img.Width;
            var height = img.Height;
            var resultBitmap = new Bitmap(width, height);

            var kHalf = kernelSize / 2;

            var bmpData = img.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            var pixelData = new byte[Math.Abs(bmpData.Stride) * height];
            Marshal.Copy(bmpData.Scan0, pixelData, 0, pixelData.Length);
            img.UnlockBits(bmpData);

            var resultData = resultBitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            var resultPixelData = new byte[Math.Abs(resultData.Stride) * height];

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var operationMet = isErosion;
                    for (var kx = -kHalf; kx <= kHalf && operationMet == isErosion; kx++)
                    {
                        for (var ky = -kHalf; ky <= kHalf && operationMet == isErosion; ky++)
                        {
                            var posX = x + kx;
                            var posY = y + ky;

                            if (posX < 0 || posX >= width || posY < 0 || posY >= height)
                            {
                                continue;
                            }

                            var index = posY * bmpData.Stride + posX * 3;
                            var avg = (byte)((pixelData[index] + pixelData[index + 1] + pixelData[index + 2]) / 3);

                            operationMet = isErosion switch
                            {
                                true when avg < threshold => false,
                                false when avg >= threshold => true,
                                _ => operationMet
                            };
                        }
                    }

                    var resultIndex = y * resultData.Stride + x * 3;
                    var color = operationMet ? (byte)255 : (byte)0;
                    resultPixelData[resultIndex] = color;
                    resultPixelData[resultIndex + 1] = color;
                    resultPixelData[resultIndex + 2] = color;
                }
            }

            Marshal.Copy(resultPixelData, 0, resultData.Scan0, resultPixelData.Length);
            resultBitmap.UnlockBits(resultData);

            return resultBitmap;
        }

        public static Bitmap Open(Bitmap img)
        {
            var erodedImage = Morphology.Erode(img);
            return Morphology.Dilate(erodedImage);
        }

        public static Bitmap Close(Bitmap img)
        {
            var dilatedImage = Morphology.Dilate(img);
            return Morphology.Erode(dilatedImage);
        }

        public static Bitmap MorphologicalGradient(Bitmap img)
        {
            var dilatedImage = Dilate(img);
            var erodedImage = Erode(img);

            var width = img.Width;
            var height = img.Height;
            var gradientImage = new Bitmap(width, height);

            var dilatedData = dilatedImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            var dilatedPixelData = new byte[Math.Abs(dilatedData.Stride) * height];
            Marshal.Copy(dilatedData.Scan0, dilatedPixelData, 0, dilatedPixelData.Length);

            var erodedData = erodedImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            var erodedPixelData = new byte[Math.Abs(erodedData.Stride) * height];
            Marshal.Copy(erodedData.Scan0, erodedPixelData, 0, erodedPixelData.Length);

            var gradientData = gradientImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            var gradientPixelData = new byte[Math.Abs(gradientData.Stride) * height];

            for (var i = 0; i < dilatedPixelData.Length; i++)
            {
                gradientPixelData[i] = (byte)Math.Abs(dilatedPixelData[i] - erodedPixelData[i]);
            }

            Marshal.Copy(gradientPixelData, 0, gradientData.Scan0, gradientPixelData.Length);
            gradientImage.UnlockBits(gradientData);

            dilatedImage.UnlockBits(dilatedData);
            erodedImage.UnlockBits(erodedData);

            return gradientImage;
        }

        public static Bitmap TopHat(Bitmap img)
        {
            var openedImage = Open(img);
            return Algebra.Subtract(img, openedImage);
        }

        public static Bitmap BottomHat(Bitmap img)
        {
            var closedImage = Close(img);
            return Algebra.Subtract(closedImage, img);
        }

        private static Bitmap ErodeUsingKernel(Bitmap img, Bitmap kernel, byte threshold = 128)
        {
            var width = img.Width;
            var height = img.Height;
            var erodedBitmap = new Bitmap(width, height);

            var kHalfWidth = kernel.Width / 2;
            var kHalfHeight = kernel.Height / 2;

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var isEroded = false;

                    for (var kx = -kHalfWidth; kx <= kHalfWidth && !isEroded; kx++)
                    {
                        for (var ky = -kHalfHeight; ky <= kHalfHeight && !isEroded; ky++)
                        {
                            var posX = x + kx;
                            var posY = y + ky;

                            // Check the bounds for the kernel
                            if (posX < 0 || posX >= width || posY < 0 || posY >= height)
                            {
                                continue;
                            }

                            var kernelColor = kernel.GetPixel(kx + kHalfWidth, ky + kHalfHeight);
                            var imageColor = img.GetPixel(posX, posY);

                            if (kernelColor.R > threshold && imageColor.R < threshold)
                            {
                                isEroded = true;
                            }
                        }
                    }

                    erodedBitmap.SetPixel(x, y, isEroded ? Color.Black : Color.White);
                }
            }

            return erodedBitmap;
        }

        public static Bitmap PerformHitOrMiss(Bitmap img, Bitmap hitKernel, Bitmap missKernel)
        {
            // Erode the original image with the "hit" kernel
            var hitResult = ErodeUsingKernel(img, hitKernel);

            // Invert the original image
            var invertedImage = Enhancement.Invert(img);

            // Erode the inverted image with the "miss" kernel
            var missResult = ErodeUsingKernel(img, missKernel);

            // Take the intersection of hitResult and missResult
            var result = Algebra.Intersect(hitResult, missResult, 128);
            return result;
        }

        public static Bitmap Skeletonize(Bitmap img)
        {
            var width = img.Width;
            var height = img.Height;
            var skeleton = new Bitmap(width, height);

            var pixelData = LockBitmapAndGetPixelData(img);
            var skeletonData = LockBitmapAndGetPixelData(skeleton);

            bool hasChanged;

            do
            {
                hasChanged = false;

                for (var y = 1; y < height - 1; y++)
                {
                    for (var x = 1; x < width - 1; x++)
                    {
                        if (pixelData[x, y] == 0)
                            continue;

                        var transitions = CountTransitions(pixelData, x, y);

                        if (transitions != 1 && transitions != 2) continue;
                        skeletonData[x, y] = 255;
                        hasChanged = true;
                    }
                }

                for (var y = 1; y < height - 1; y++)
                {
                    for (var x = 1; x < width - 1; x++)
                    {
                        if (skeletonData[x, y] == 0)
                            continue;

                        var transitions = CountTransitions(pixelData, x, y);

                        if (transitions != 1 && transitions != 2) continue;
                        skeletonData[x, y] = 0;
                        hasChanged = true;
                    }
                }
            } while (hasChanged);

            UnlockBitmap(pixelData);
            UnlockBitmap(skeletonData);

            return skeleton;
        }

        private static byte[,] LockBitmapAndGetPixelData(Bitmap bmp)
        {
            var width = bmp.Width;
            var height = bmp.Height;
            var data = new byte[width, height];

            var bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);

            unsafe
            {
                var scan0 = (byte*)bmpData.Scan0;

                for (var y = 0; y < height; y++)
                {
                    for (var x = 0; x < width; x++)
                    {
                        data[x, y] = scan0[x + y * bmpData.Stride];
                    }
                }
            }

            bmp.UnlockBits(bmpData);
            return data;
        }

        private static void UnlockBitmap(byte[,] data)
        {
            GC.KeepAlive(data);
        }

        private static int CountTransitions(byte[,] pixelData, int x, int y)
        {
            var neighbors = new byte[8];
            neighbors[0] = pixelData[x - 1, y];
            neighbors[1] = pixelData[x - 1, y + 1];
            neighbors[2] = pixelData[x, y + 1];
            neighbors[3] = pixelData[x + 1, y + 1];
            neighbors[4] = pixelData[x + 1, y];
            neighbors[5] = pixelData[x + 1, y - 1];
            neighbors[6] = pixelData[x, y - 1];
            neighbors[7] = pixelData[x - 1, y - 1];

            var transitions = 0;

            for (var i = 0; i < 8; i++)
            {
                if (neighbors[i] == 0 && neighbors[(i + 1) % 8] == 255)
                    transitions++;
            }

            return transitions;
        }

        public static Bitmap Thin(Bitmap img, Bitmap hitKernel, Bitmap missKernel)
        {
            var hitOrMissResult = PerformHitOrMiss(img, hitKernel, missKernel);
            return Algebra.Subtract(img, hitOrMissResult);
        }

        public static Bitmap Thicken(Bitmap img, Bitmap hitKernel, Bitmap missKernel)
        {
            var hitOrMissResult = PerformHitOrMiss(img, hitKernel, missKernel);
            return Algebra.Union(img, hitOrMissResult);
        }

        public static Bitmap Prune(Bitmap img, int iterations)
        {
            var prunedImage = (Bitmap)img.Clone();

            for (var i = 0; i < iterations; i++)
            {
                var endpoints = DetectEndpoints(prunedImage);
                prunedImage = Algebra.Subtract(prunedImage, endpoints);
            }

            return prunedImage;
        }

        private static Bitmap DetectEndpoints(Bitmap img)
        {
            // The kernel for endpoint detection in hit-or-miss transform
            var hitKernel = new Bitmap(3, 3);
            var g = Graphics.FromImage(hitKernel);
            g.Clear(Color.Black);
            hitKernel.SetPixel(1, 1, Color.White); // center
            g.Dispose();

            var missKernel = new Bitmap(3, 3);
            g = Graphics.FromImage(missKernel);
            g.Clear(Color.White);
            missKernel.SetPixel(1, 1, Color.Black); // center
            g.Dispose();

            // Adding more patterns to the missKernel for detecting endpoints
            // We are ensuring that the center is a skeleton point and it has just one neighbor.
            int[] neighborsX = { 0, 1, 1, 1, 0, -1, -1, -1 };
            int[] neighborsY = { -1, -1, 0, 1, 1, 1, 0, -1 };

            for (var i = 0; i < 8; i++)
            {
                missKernel.SetPixel(1 + neighborsX[i], 1 + neighborsY[i], Color.Black);
                var result = PerformHitOrMiss(img, hitKernel, missKernel);
                if (i != 7)
                    missKernel.SetPixel(1 + neighborsX[i], 1 + neighborsY[i], Color.White);
            }

            return PerformHitOrMiss(img, hitKernel, missKernel);
        }
    }
}