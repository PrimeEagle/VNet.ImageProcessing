using System.Drawing;
using System.Drawing.Imaging;
#pragma warning disable CA1416

namespace VNet.ImageProcessing
{
    public static class Segmentation
    {
        public static Bitmap ConnectedComponentAnalysis(Bitmap input, ConnectedComponentAnalysisConnectivityType connectivity = ConnectedComponentAnalysisConnectivityType.FourNeighbors)
        {
            var width = input.Width;
            var height = input.Height;

            var labels = new int[width * height];
            var nextLabel = 1;

            var linked = new Dictionary<int, int>();

            var inputData = input.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            unsafe
            {
                var ptr = (byte*)inputData.Scan0;

                // First pass
                for (var y = 0; y < height; y++)
                {
                    for (var x = 0; x < width; x++)
                    {
                        var idx = y * width + x;
                        var pixelPos = y * inputData.Stride + x * 3;
                        if (ptr[pixelPos] == 255)  // Check if the pixel is white (foreground)
                        {
                            var neighbors = new List<int>();

                            // 4-connectivity
                            if (x > 0) neighbors.Add(labels[idx - 1]);
                            if (y > 0) neighbors.Add(labels[idx - width]);

                            // 8-connectivity
                            if (connectivity == ConnectedComponentAnalysisConnectivityType.EightNeighbors)
                            {
                                if (x > 0 && y > 0) neighbors.Add(labels[idx - width - 1]);
                                if (x < width - 1 && y > 0) neighbors.Add(labels[idx - width + 1]);
                            }

                            neighbors.RemoveAll(label => label == 0);

                            if (neighbors.Count == 0)
                            {
                                labels[idx] = nextLabel;
                                nextLabel++;
                            }
                            else
                            {
                                labels[idx] = neighbors.Min();
                                foreach (var label in neighbors.Where(label => label != labels[idx]))
                                {
                                    linked[label] = labels[idx];
                                }
                            }
                        }
                    }
                }

                // Second pass
                for (var y = 0; y < height; y++)
                {
                    for (var x = 0; x < width; x++)
                    {
                        var idx = y * width + x;
                        if (labels[idx] != 0)
                        {
                            var curLabel = labels[idx];
                            while (linked.ContainsKey(curLabel))
                            {
                                curLabel = linked[curLabel];
                            }
                            labels[idx] = curLabel;
                        }
                    }
                }
            }

            input.UnlockBits(inputData);

            // Creating output image with colored components
            var output = new Bitmap(width, height);
            var rand = new Random();
            var colorMap = new Dictionary<int, Color>();

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var label = labels[y * width + x];
                    if (label != 0)
                    {
                        if (!colorMap.ContainsKey(label))
                        {
                            colorMap[label] = Color.FromArgb(rand.Next(256), rand.Next(256), rand.Next(256));
                        }
                        output.SetPixel(x, y, colorMap[label]);
                    }
                    else
                    {
                        output.SetPixel(x, y, Color.Black);
                    }
                }
            }

            return output;
        }
    }
}