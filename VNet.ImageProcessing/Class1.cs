using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace VNet.ImageProcessing
{
    public class Util
    {
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

    using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

public class MorphologicalProcessing
    {
        private Bitmap _originalBitmap;
        private int _kernelSize;
        private byte _threshold;

        public MorphologicalProcessing(Bitmap bitmap, int kernelSize = 3, byte threshold = 128)
        {
            _originalBitmap = bitmap;
            _kernelSize = kernelSize;
            _threshold = threshold;
        }

        public Bitmap PerformErosion()
        {
            return PerformMorphologicalOperation(true);
        }

        public Bitmap PerformDilation()
        {
            return PerformMorphologicalOperation(false);
        }

        private Bitmap PerformMorphologicalOperation(bool isErosion)
        {
            var width = _originalBitmap.Width;
            var height = _originalBitmap.Height;
            var resultBitmap = new Bitmap(width, height);

            var kHalf = _kernelSize / 2;

            var bmpData = _originalBitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            var pixelData = new byte[Math.Abs(bmpData.Stride) * height];
            Marshal.Copy(bmpData.Scan0, pixelData, 0, pixelData.Length);
            _originalBitmap.UnlockBits(bmpData);

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

                            if (isErosion && avg < _threshold) operationMet = false;
                            if (!isErosion && avg >= _threshold) operationMet = true;
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

        public Bitmap PerformOpening()
        {
            var erodedImage = PerformErosion();
            var dilationAfterErosion = new MorphologicalProcessing(erodedImage, _kernelSize, _threshold);
            return dilationAfterErosion.PerformDilation();
        }

        public Bitmap PerformClosing()
        {
            var dilatedImage = PerformDilation();
            var erosionAfterDilation = new MorphologicalProcessing(dilatedImage, _kernelSize, _threshold);
            return erosionAfterDilation.PerformErosion();
        }

        public Bitmap PerformMorphologicalGradient()
        {
            var dilatedImage = PerformDilation();
            var erodedImage = PerformErosion();

            var width = _originalBitmap.Width;
            var height = _originalBitmap.Height;
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

        public Bitmap PerformTopHat()
        {
            var openedImage = PerformOpening();
            return SubtractImages(_originalBitmap, openedImage);
        }

        public Bitmap PerformBottomHat()
        {
            var closedImage = PerformClosing();
            return SubtractImages(closedImage, _originalBitmap);
        }

        private Bitmap SubtractImages(Bitmap image1, Bitmap image2)
        {
            var width = image1.Width;
            var height = image1.Height;

            var resultImage = new Bitmap(width, height);

            var data1 = image1.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            var pixelData1 = new byte[Math.Abs(data1.Stride) * height];
            Marshal.Copy(data1.Scan0, pixelData1, 0, pixelData1.Length);

            var data2 = image2.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            var pixelData2 = new byte[Math.Abs(data2.Stride) * height];
            Marshal.Copy(data2.Scan0, pixelData2, 0, pixelData2.Length);

            var resultData = resultImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            var resultPixelData = new byte[Math.Abs(resultData.Stride) * height];

            for (var i = 0; i < pixelData1.Length; i++)
            {
                resultPixelData[i] = (byte)Math.Abs(pixelData1[i] - pixelData2[i]);
            }

            Marshal.Copy(resultPixelData, 0, resultData.Scan0, resultPixelData.Length);
            resultImage.UnlockBits(resultData);

            image1.UnlockBits(data1);
            image2.UnlockBits(data2);

            return resultImage;
        }

        private Bitmap InvertImage(Bitmap image)
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

        private Bitmap IntersectImages(Bitmap image1, Bitmap image2)
        {
            var width = image1.Width;
            var height = image1.Height;
            var result = new Bitmap(width, height);

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var pixel1 = image1.GetPixel(x, y);
                    var pixel2 = image2.GetPixel(x, y);

                    // If both pixels are white, set the result pixel to white
                    if (pixel1.R > _threshold && pixel2.R > _threshold)
                    {
                        result.SetPixel(x, y, Color.White);
                    }
                    else
                    {
                        result.SetPixel(x, y, Color.Black);
                    }
                }
            }

            return result;
        }

        private Bitmap PerformErosionUsingKernel(Bitmap kernel)
        {
            var width = _originalBitmap.Width;
            var height = _originalBitmap.Height;
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
                            var imageColor = _originalBitmap.GetPixel(posX, posY);

                            if (kernelColor.R > _threshold && imageColor.R < _threshold)
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

        public Bitmap PerformHitOrMiss(Bitmap hitKernel, Bitmap missKernel)
        {
            // Erode the original image with the "hit" kernel
            var hitProcessor = new MorphologicalProcessing(_originalBitmap, hitKernel.Width, _threshold);
            var hitResult = hitProcessor.PerformErosionUsingKernel(hitKernel);

            // Invert the original image
            var invertedImage = InvertImage(_originalBitmap);

            // Erode the inverted image with the "miss" kernel
            var missProcessor = new MorphologicalProcessing(invertedImage, missKernel.Width, _threshold);
            var missResult = missProcessor.PerformErosionUsingKernel(missKernel);

            // Take the intersection of hitResult and missResult
            var result = IntersectImages(hitResult, missResult);
            return result;
        }

        public Bitmap PerformSkeletonization()
        {
            var skel = new Bitmap(_originalBitmap.Width, _originalBitmap.Height);
            var temp = new Bitmap(_originalBitmap.Width, _originalBitmap.Height);
            Bitmap eroded;
            Bitmap opened;

            var done = false;

            while (!done)
            {
                eroded = PerformErosion();
                opened = new MorphologicalProcessing(eroded, _kernelSize, _threshold).PerformOpening();

                SubtractImages(_originalBitmap, opened, temp);
                UnionImages(skel, temp, skel);

                done = ImagesEqual(eroded, new Bitmap(eroded.Width, eroded.Height));

                _originalBitmap = eroded;
            }

            return skel;
        }

        private void SubtractImages(Bitmap image1, Bitmap image2, Bitmap result)
        {
            var data1 = image1.LockBits(new Rectangle(0, 0, image1.Width, image1.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            var data2 = image2.LockBits(new Rectangle(0, 0, image2.Width, image2.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            var resultData = result.LockBits(new Rectangle(0, 0, result.Width, result.Height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            var bytes = Math.Abs(data1.Stride) * image1.Height;
            var buffer1 = new byte[bytes];
            var buffer2 = new byte[bytes];
            var resultBuffer = new byte[bytes];

            Marshal.Copy(data1.Scan0, buffer1, 0, bytes);
            Marshal.Copy(data2.Scan0, buffer2, 0, bytes);

            for (var i = 0; i < bytes; i += 3)  // Assuming 24bpp image, so increment by 3 for each pixel
            {
                resultBuffer[i] = (byte)((buffer1[i] > _threshold && buffer2[i] < _threshold) ? 255 : 0);
                resultBuffer[i + 1] = resultBuffer[i];  // Copying the same value for green and blue channels
                resultBuffer[i + 2] = resultBuffer[i];
            }

            Marshal.Copy(resultBuffer, 0, resultData.Scan0, bytes);

            image1.UnlockBits(data1);
            image2.UnlockBits(data2);
            result.UnlockBits(resultData);
        }

        private void UnionImages(Bitmap image1, Bitmap image2, Bitmap result)
        {
            var data1 = image1.LockBits(new Rectangle(0, 0, image1.Width, image1.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            var data2 = image2.LockBits(new Rectangle(0, 0, image2.Width, image2.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            var resultData = result.LockBits(new Rectangle(0, 0, result.Width, result.Height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            var bytes = Math.Abs(data1.Stride) * image1.Height;
            var buffer1 = new byte[bytes];
            var buffer2 = new byte[bytes];
            var resultBuffer = new byte[bytes];

            Marshal.Copy(data1.Scan0, buffer1, 0, bytes);
            Marshal.Copy(data2.Scan0, buffer2, 0, bytes);

            for (var i = 0; i < bytes; i += 3)  // Assuming 24bpp image, so increment by 3 for each pixel
            {
                resultBuffer[i] = (byte)((buffer1[i] > _threshold || buffer2[i] > _threshold) ? 255 : 0);
                resultBuffer[i + 1] = resultBuffer[i];  // Copying the same value for green and blue channels
                resultBuffer[i + 2] = resultBuffer[i];
            }

            Marshal.Copy(resultBuffer, 0, resultData.Scan0, bytes);

            image1.UnlockBits(data1);
            image2.UnlockBits(data2);
            result.UnlockBits(resultData);
        }

        private bool ImagesEqual(Bitmap img1, Bitmap img2)
        {
            var data1 = img1.LockBits(new Rectangle(0, 0, img1.Width, img1.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            var data2 = img2.LockBits(new Rectangle(0, 0, img2.Width, img2.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            var bytes = Math.Abs(data1.Stride) * img1.Height;
            var buffer1 = new byte[bytes];
            var buffer2 = new byte[bytes];

            Marshal.Copy(data1.Scan0, buffer1, 0, bytes);
            Marshal.Copy(data2.Scan0, buffer2, 0, bytes);

            for (var i = 0; i < bytes; i++)
            {
                if (buffer1[i] != buffer2[i])
                {
                    img1.UnlockBits(data1);
                    img2.UnlockBits(data2);
                    return false;
                }
            }

            img1.UnlockBits(data1);
            img2.UnlockBits(data2);

            return true;
        }

        public Bitmap PerformThinning(Bitmap hitKernel, Bitmap missKernel)
        {
            var hitOrMissResult = PerformHitOrMiss(hitKernel, missKernel);
            return SubtractImagesUsingBitmapData(_originalBitmap, hitOrMissResult);
        }

        public Bitmap PerformThickening(Bitmap hitKernel, Bitmap missKernel)
        {
            var hitOrMissResult = PerformHitOrMiss(hitKernel, missKernel);
            return UnionImagesUsingBitmapData(_originalBitmap, hitOrMissResult);
        }

        private Bitmap SubtractImagesUsingBitmapData(Bitmap image1, Bitmap image2)
        {
            var width = image1.Width;
            var height = image1.Height;
            var result = new Bitmap(width, height);

            var data1 = image1.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            var data2 = image2.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            var resultData = result.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            var bytes = Math.Abs(data1.Stride) * height;
            var buffer1 = new byte[bytes];
            var buffer2 = new byte[bytes];
            var resultBuffer = new byte[bytes];

            Marshal.Copy(data1.Scan0, buffer1, 0, bytes);
            Marshal.Copy(data2.Scan0, buffer2, 0, bytes);

            for (var i = 0; i < bytes; i += 3)
            {
                resultBuffer[i] = (byte)((buffer1[i] > _threshold && buffer2[i] < _threshold) ? 255 : 0);
                resultBuffer[i + 1] = resultBuffer[i];
                resultBuffer[i + 2] = resultBuffer[i];
            }

            Marshal.Copy(resultBuffer, 0, resultData.Scan0, bytes);

            image1.UnlockBits(data1);
            image2.UnlockBits(data2);
            result.UnlockBits(resultData);

            return result;
        }

        private Bitmap UnionImagesUsingBitmapData(Bitmap image1, Bitmap image2)
        {
            var width = image1.Width;
            var height = image1.Height;
            var result = new Bitmap(width, height);

            var data1 = image1.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            var data2 = image2.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            var resultData = result.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            var bytes = Math.Abs(data1.Stride) * height;
            var buffer1 = new byte[bytes];
            var buffer2 = new byte[bytes];
            var resultBuffer = new byte[bytes];

            Marshal.Copy(data1.Scan0, buffer1, 0, bytes);
            Marshal.Copy(data2.Scan0, buffer2, 0, bytes);

            for (var i = 0; i < bytes; i += 3)
            {
                resultBuffer[i] = (byte)((buffer1[i] > _threshold || buffer2[i] > _threshold) ? 255 : 0);
                resultBuffer[i + 1] = resultBuffer[i];
                resultBuffer[i + 2] = resultBuffer[i];
            }

            Marshal.Copy(resultBuffer, 0, resultData.Scan0, bytes);

            image1.UnlockBits(data1);
            image2.UnlockBits(data2);
            result.UnlockBits(resultData);

            return result;
        }

        public Bitmap PerformPruning(int iterations)
        {
            var prunedImage = (Bitmap)_originalBitmap.Clone();

            for (var i = 0; i < iterations; i++)
            {
                var endpoints = DetectEndpoints(prunedImage);
                prunedImage = SubtractImagesUsingBitmapData(prunedImage, endpoints);
            }

            return prunedImage;
        }

        private Bitmap DetectEndpoints(Bitmap image)
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
                var result = PerformHitOrMiss(hitKernel, missKernel);
                if (i != 7)
                    missKernel.SetPixel(1 + neighborsX[i], 1 + neighborsY[i], Color.White);
            }

            return PerformHitOrMiss(hitKernel, missKernel);
        }
    }
}