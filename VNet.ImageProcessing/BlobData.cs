using System.Drawing;

namespace VNet.ImageProcessing
{
    public class BlobData
    {
        public int Area { get; set; }
        public Point Centroid { get; set; }
        public Rectangle BoundingBox { get; set; }
    }
}