using System.Drawing;

namespace VNet.ImageProcessing;

public static class SegmentationExtensions
{
    public static Bitmap ConnectedComponentAnalysis(this Bitmap img, ConnectedComponentAnalysisConnectivityType connectivity = ConnectedComponentAnalysisConnectivityType.FourNeighbors)
    {
        return Segmentation.ConnectedComponentAnalysis(img, connectivity);
    }

    public static Dictionary<Color, BlobData> BlobCounter(this Bitmap img)
    {
        return Segmentation.BlobCounter(img);
    }

    public static (Dictionary<Color, BlobData>, Bitmap) FilterBlobs(this Bitmap img, Dictionary<Color, BlobData> blobs, int areaFilterValue, FilterType filterType)
    {
        return Segmentation.FilterBlobs(img, blobs, areaFilterValue, filterType);
    }
}