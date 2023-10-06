using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace VNet.ImageProcessing.Algebra
{
    public class Class1
    {
        public static Bitmap AddBitmaps(Bitmap img1, Bitmap img2)
        {
            var width = img1.Width;
            var height = img1.Height;

            var result = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            var data1 = img1.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            var data2 = img2.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            var resultData = result.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            var bytes = Math.Abs(data1.Stride) * height;
            var buffer1 = new byte[bytes];
            var buffer2 = new byte[bytes];
            var resultBuffer = new byte[bytes];

            Marshal.Copy(data1.Scan0, buffer1, 0, bytes);
            Marshal.Copy(data2.Scan0, buffer2, 0, bytes);

            for (var i = 0; i < bytes; i++)
            {
                var val = buffer1[i] + buffer2[i];
                resultBuffer[i] = (byte)Math.Min(val, 255);  // Ensure values are clamped to [0, 255]
            }

            Marshal.Copy(resultBuffer, 0, resultData.Scan0, bytes);

            img1.UnlockBits(data1);
            img2.UnlockBits(data2);
            result.UnlockBits(resultData);

            return result;
        }

        public static Bitmap SubtractBitmaps(Bitmap img1, Bitmap img2)
        {
            // Similar structure to AddBitmaps.
            // Replace the core loop logic with:
            // int val = buffer1[i] - buffer2[i];
            // resultBuffer[i] = (byte)Math.Max(val, 0);  // Ensure values are clamped to [0, 255]
            // ...

            // Return the result
        }

        public static Bitmap MultiplyBitmaps(Bitmap img1, Bitmap img2)
        {
            // Similar structure to AddBitmaps.
            // Replace the core loop logic with:
            // int val = (buffer1[i] * buffer2[i]) / 255;  // Normalize to [0, 255]
            // resultBuffer[i] = (byte)val;
            // ...

            // Return the result
        }

        public static Bitmap DivideBitmaps(Bitmap img1, Bitmap img2)
        {
            // Similar structure to AddBitmaps.
            // Replace the core loop logic with:
            // int val = buffer2[i] == 0 ? 255 : (buffer1[i] * 255) / buffer2[i];  // Avoid division by zero and normalize to [0, 255]
            // resultBuffer[i] = (byte)val;
            // ...

            // Return the result
        }

        public static Bitmap BitwiseAnd(Bitmap img1, Bitmap img2)
        {
            return BitwiseOperation(img1, img2, (byte1, byte2) => (byte)(byte1 & byte2));
        }

        public static Bitmap BitwiseOr(Bitmap img1, Bitmap img2)
        {
            return BitwiseOperation(img1, img2, (byte1, byte2) => (byte)(byte1 | byte2));
        }

        public static Bitmap BitwiseXor(Bitmap img1, Bitmap img2)
        {
            return BitwiseOperation(img1, img2, (byte1, byte2) => (byte)(byte1 ^ byte2));
        }

        public static Bitmap BitwiseNot(Bitmap img)
        {
            var width = img.Width;
            var height = img.Height;
            var result = new Bitmap(width, height, PixelFormat.Format24bppRgb);

            var data = img.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            var resultData = result.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            var bytes = Math.Abs(data.Stride) * height;
            var buffer = new byte[bytes];
            var resultBuffer = new byte[bytes];

            Marshal.Copy(data.Scan0, buffer, 0, bytes);

            for (var i = 0; i < bytes; i++)
            {
                resultBuffer[i] = (byte)(~buffer[i]);
            }

            Marshal.Copy(resultBuffer, 0, resultData.Scan0, bytes);

            img.UnlockBits(data);
            result.UnlockBits(resultData);

            return result;
        }

        private static Bitmap BitwiseOperation(Bitmap img1, Bitmap img2, Func<byte, byte, byte> operation)
        {
            var width = img1.Width;
            var height = img1.Height;

            if (img1.Width != img2.Width || img1.Height != img2.Height)
            {
                throw new ArgumentException("Images must have the same dimensions.");
            }

            var result = new Bitmap(width, height, PixelFormat.Format24bppRgb);

            var data1 = img1.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            var data2 = img2.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            var resultData = result.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            var bytes = Math.Abs(data1.Stride) * height;
            var buffer1 = new byte[bytes];
            var buffer2 = new byte[bytes];
            var resultBuffer = new byte[bytes];

            Marshal.Copy(data1.Scan0, buffer1, 0, bytes);
            Marshal.Copy(data2.Scan0, buffer2, 0, bytes);

            for (var i = 0; i < bytes; i++)
            {
                resultBuffer[i] = operation(buffer1[i], buffer2[i]);
            }

            Marshal.Copy(resultBuffer, 0, resultData.Scan0, bytes);

            img1.UnlockBits(data1);
            img2.UnlockBits(data2);
            result.UnlockBits(resultData);

            return result;
        }

        public static Bitmap ApplyThreshold(Bitmap image, byte threshold, byte highValue = 255, byte lowValue = 0)
        {
            var width = image.Width;
            var height = image.Height;
            var result = new Bitmap(width, height, PixelFormat.Format24bppRgb);

            var imageData = image.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            var resultData = result.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            var bytes = Math.Abs(imageData.Stride) * height;
            var buffer = new byte[bytes];
            var resultBuffer = new byte[bytes];

            Marshal.Copy(imageData.Scan0, buffer, 0, bytes);

            for (var i = 0; i < bytes; i += 3) // Assuming 24bpp image
            {
                var grayValue = (byte)(0.3 * buffer[i + 2] + 0.59 * buffer[i + 1] + 0.11 * buffer[i]); // Convert to grayscale using common weights

                if (grayValue >= threshold)
                {
                    resultBuffer[i] = highValue;
                    resultBuffer[i + 1] = highValue;
                    resultBuffer[i + 2] = highValue;
                }
                else
                {
                    resultBuffer[i] = lowValue;
                    resultBuffer[i + 1] = lowValue;
                    resultBuffer[i + 2] = lowValue;
                }
            }

            Marshal.Copy(resultBuffer, 0, resultData.Scan0, bytes);

            image.UnlockBits(imageData);
            result.UnlockBits(resultData);

            return result;
        }

        public static Bitmap ApplyDistanceTransform(Bitmap image)
        {
            var width = image.Width;
            var height = image.Height;

            // Intermediate array for storing raw distances.
            var distance = new int[width, height];

            // First pass
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    if (image.GetPixel(x, y).R == 255) // White pixel
                    {
                        var up = (y > 0) ? distance[x, y - 1] + 1 : int.MaxValue;
                        var left = (x > 0) ? distance[x - 1, y] + 1 : int.MaxValue;
                        distance[x, y] = Math.Min(up, left);
                    }
                    else
                    {
                        distance[x, y] = 0; // For black pixels
                    }
                }
            }

            // Second pass
            for (var y = height - 1; y >= 0; y--)
            {
                for (var x = width - 1; x >= 0; x--)
                {
                    var down = (y < height - 1) ? distance[x, y + 1] + 1 : int.MaxValue;
                    var right = (x < width - 1) ? distance[x + 1, y] + 1 : int.MaxValue;
                    distance[x, y] = Math.Min(distance[x, y], Math.Min(down, right));
                }
            }

            // Convert distances to grayscale image
            var result = new Bitmap(width, height);
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var value = (byte)Math.Min(distance[x, y], 255); // Clamp to byte range
                    result.SetPixel(x, y, Color.FromArgb(value, value, value));
                }
            }

            return result;
        }
    }
}
