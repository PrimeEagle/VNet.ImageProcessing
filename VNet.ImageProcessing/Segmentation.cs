﻿using System.Diagnostics.Metrics;
using System.Drawing;
using System.Drawing.Imaging;
#pragma warning disable IDE0060
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
                        if (ptr[pixelPos] == 255) // Check if the pixel is white (foreground)
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

        public static Dictionary<Color, BlobData> BlobCounter(Bitmap img)
        {
            var bmpData = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            var blobs = new Dictionary<Color, BlobData>();
            var processedColors = new HashSet<Color>();

            unsafe
            {
                var ptr = (byte*)bmpData.Scan0;

                for (var y = 0; y < bmpData.Height; y++)
                {
                    for (var x = 0; x < bmpData.Width; x++)
                    {
                        var idx = y * bmpData.Stride + x * 3;
                        var currentColor = Color.FromArgb(ptr[idx + 2], ptr[idx + 1], ptr[idx]);

                        if (currentColor == Color.Black || processedColors.Contains(currentColor)) continue;  // skip background and already processed blobs

                        if (!blobs.ContainsKey(currentColor))
                        {
                            blobs[currentColor] = new BlobData
                            {
                                Area = 0,
                                Centroid = new Point(0, 0),
                                BoundingBox = new Rectangle(x, y, 1, 1),
                                BoundaryPoints = GetBoundaryPoints(img, new Point(x, y), currentColor)
                            };
                            processedColors.Add(currentColor);
                        }

                        var blob = blobs[currentColor];

                        blob.Area++;
                        blob.Centroid = new Point(blob.Centroid.X + x, blob.Centroid.Y + y);
                        blob.BoundingBox = Rectangle.Union(blob.BoundingBox, new Rectangle(x, y, 1, 1));
                    }
                }
            }

            img.UnlockBits(bmpData);

            foreach (var blob in blobs.Values)
            {
                blob.Centroid = new Point(blob.Centroid.X / blob.Area, blob.Centroid.Y / blob.Area);
            }

            return blobs;
        }

        private static List<Point> GetBoundaryPoints(Bitmap img, Point start, Color targetColor)
        {
            // Moore-Neighbor Tracing algorithm
            var boundary = new List<Point>();
            var current = start;
            var next = start;
            var previous = start;

            do
            {
                boundary.Add(current);
                var neighbors = Get8Neighbors(current);

                var startIndex = GetNeighborIndex(previous, current);
                for (var i = 0; i < 8; i++)
                {
                    var index = (startIndex + i) % 8;
                    if (img.GetPixel(neighbors[index].X, neighbors[index].Y) == targetColor)
                    {
                        next = neighbors[index];
                        previous = current;
                        break;
                    }
                }

                if (next == current)
                    break;  // isolated pixel

                current = next;

            } while (current != start);

            return boundary;
        }

        private static Point[] Get8Neighbors(Point p)
        {
            return new Point[]
            {
                new Point(p.X, p.Y - 1),      // Top
                new Point(p.X + 1, p.Y - 1),  // Top-Right
                new Point(p.X + 1, p.Y),      // Right
                new Point(p.X + 1, p.Y + 1),  // Bottom-Right
                new Point(p.X, p.Y + 1),      // Bottom
                new Point(p.X - 1, p.Y + 1),  // Bottom-Left
                new Point(p.X - 1, p.Y),      // Left
                new Point(p.X - 1, p.Y - 1)   // Top-Left
            };
        }

        private static int GetNeighborIndex(Point previous, Point current)
        {
            var diff = new Point(current.X - previous.X, current.Y - previous.Y);
            if (diff == new Point(0, -1)) return 0;
            if (diff == new Point(1, -1)) return 1;
            if (diff == new Point(1, 0)) return 2;
            if (diff == new Point(1, 1)) return 3;
            if (diff == new Point(0, 1)) return 4;
            if (diff == new Point(-1, 1)) return 5;
            if (diff == new Point(-1, 0)) return 6;
            return diff == new Point(-1, -1) ? 7 : 0; // default to 0 if no match (shouldn't happen in proper context)
        }

        public static (Dictionary<Color, BlobData>, Bitmap) FilterBlobs(Bitmap img, Dictionary<Color, BlobData> blobs, int areaFilterValue, FilterType filterType)
        {
            var filteredBlobs = new Dictionary<Color, BlobData>();
            var filteredBitmap = new Bitmap(img.Width, img.Height);

            using (var g = Graphics.FromImage(filteredBitmap))
            {
                g.Clear(Color.Black);  // Setting background color to black
            }

            var inputData = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            var outputData = filteredBitmap.LockBits(new Rectangle(0, 0, filteredBitmap.Width, filteredBitmap.Height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            unsafe
            {
                var inputPtr = (byte*)inputData.Scan0;
                var outputPtr = (byte*)outputData.Scan0;

                for (var y = 0; y < img.Height; y++)
                {
                    for (var x = 0; x < img.Width; x++)
                    {
                        var idx = y * inputData.Stride + x * 3;
                        var currentColor = Color.FromArgb(inputPtr[idx + 2], inputPtr[idx + 1], inputPtr[idx]);

                        if (!blobs.ContainsKey(currentColor)) continue;

                        var meetsCriteria = filterType switch
                        {
                            FilterType.GreaterThan => blobs[currentColor].Area > areaFilterValue,
                            FilterType.LessThan => blobs[currentColor].Area < areaFilterValue,
                            FilterType.EqualTo => blobs[currentColor].Area == areaFilterValue,
                            _ => false
                        };

                        if (!meetsCriteria) continue;
                        filteredBlobs.TryAdd(currentColor, blobs[currentColor]);

                        outputPtr[idx] = inputPtr[idx];
                        outputPtr[idx + 1] = inputPtr[idx + 1];
                        outputPtr[idx + 2] = inputPtr[idx + 2];
                    }
                }
            }

            img.UnlockBits(inputData);
            filteredBitmap.UnlockBits(outputData);

            return (filteredBlobs, filteredBitmap);
        }
    }
}