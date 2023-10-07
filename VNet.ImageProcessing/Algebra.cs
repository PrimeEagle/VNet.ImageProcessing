using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
#pragma warning disable CA1416

namespace VNet.ImageProcessing
{
    public static class Algebra
    {
        public static Bitmap Add(Bitmap img1, Bitmap img2)
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
                resultBuffer[i] = ImageUtil.Clamp(val);
            }

            Marshal.Copy(resultBuffer, 0, resultData.Scan0, bytes);

            img1.UnlockBits(data1);
            img2.UnlockBits(data2);
            result.UnlockBits(resultData);

            return result;
        }

        public static Bitmap Subtract(Bitmap img1, Bitmap img2)
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
                var val = buffer1[i] - buffer2[i];
                resultBuffer[i] = ImageUtil.Clamp(val);
            }

            Marshal.Copy(resultBuffer, 0, resultData.Scan0, bytes);

            img1.UnlockBits(data1);
            img2.UnlockBits(data2);
            result.UnlockBits(resultData);

            return result;
        }

        public static Bitmap Multiply(Bitmap img1, Bitmap img2)
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
                var val = (buffer1[i] * buffer2[i]) / 255;
                resultBuffer[i] = (byte)val;
            }

            Marshal.Copy(resultBuffer, 0, resultData.Scan0, bytes);

            img1.UnlockBits(data1);
            img2.UnlockBits(data2);
            result.UnlockBits(resultData);

            return result;
        }

        public static Bitmap Divide(Bitmap img1, Bitmap img2)
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
                var val = buffer2[i] == 0 ? 255 : (buffer1[i] * 255) / buffer2[i];
                resultBuffer[i] = (byte)val;
            }

            Marshal.Copy(resultBuffer, 0, resultData.Scan0, bytes);

            img1.UnlockBits(data1);
            img2.UnlockBits(data2);
            result.UnlockBits(resultData);

            return result;
        }

        public static Bitmap Union(Bitmap img1, Bitmap img2)
        {
            return BitwiseOr(img1, img2);
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
                resultBuffer[i] = (byte)~buffer[i];
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

        public static Bitmap Intersect(Bitmap image1, Bitmap image2, byte threshold)
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
                    if (pixel1.R > threshold && pixel2.R > threshold)
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

        public static bool AreEqual(Bitmap img1, Bitmap img2)
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
    }
}