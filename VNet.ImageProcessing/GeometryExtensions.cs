using System.Drawing;

namespace VNet.ImageProcessing;

public static class GeometryExtensions
{
    public static Bitmap FlipHorizontal(this Bitmap image)
    {
        return Geometry.FlipHorizontal(image);
    }

    public static Bitmap FlipVertical(this Bitmap image)
    {
        return Geometry.FlipVertical(image);
    }

    public static Bitmap Rotate90Degrees(this Bitmap image)
    {
        return Geometry.Rotate90Degrees(image);
    }

    public static Bitmap Scale(this Bitmap image, double scaleFactor)
    {
        return Geometry.Scale(image, scaleFactor);
    }

    public static Bitmap Crop(this Bitmap image, Rectangle cropRectangle)
    {
        return Geometry.Crop(image, cropRectangle);
    }
}