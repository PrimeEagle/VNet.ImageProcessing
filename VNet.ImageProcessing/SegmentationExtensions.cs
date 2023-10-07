using System.Drawing;

namespace VNet.ImageProcessing;

public static class SegmentationExtensions
{
    public static Bitmap ConnectedComponentAnalysis(this Bitmap img, ConnectedComponentAnalysisConnectivityType connectivity = ConnectedComponentAnalysisConnectivityType.FourNeighbors)
    {
        return Segmentation.ConnectedComponentAnalysis(img, connectivity);
    }
}