using System.Drawing;

namespace VNet.ImageProcessing;

public static class MorphologyExtensions
{
    public static Bitmap Erode(this Bitmap img, Bitmap? kernel = null)
    {
        return Morphology.Erode(img, kernel);
    }

    public static Bitmap Dilate(this Bitmap img, Bitmap? kernel = null)
    {
        return Morphology.Dilate(img, kernel);
    }
    public static Bitmap Open(this Bitmap img, Bitmap? kernel = null)
    {
        return Morphology.Open(img, kernel);
    }

    public static Bitmap Close(this Bitmap img, Bitmap? kernel = null)
    {
        return Morphology.Close(img, kernel);
    }

    public static Bitmap MorphologicalGradient(this Bitmap img, Bitmap? kernel = null)
    {
        return Morphology.MorphologicalGradient(img, kernel);
    }

    public static Bitmap TopHat(this Bitmap img)
    {
        return Morphology.TopHat(img);
    }

    public static Bitmap BottomHat(this Bitmap img)
    {
        return Morphology.BottomHat(img);
    }

    public static Bitmap Skeletonize(this Bitmap img)
    {
        return Morphology.Skeletonize(img);
    }

    public static Bitmap HitOrMiss(this Bitmap img, Bitmap hitKernel, Bitmap missKernel)
    {
        return Morphology.HitOrMiss(img, hitKernel, missKernel);
    }

    public static Bitmap Thin(this Bitmap img, Bitmap hitKernel, Bitmap missKernel)
    {
        return Morphology.Thin(img, hitKernel, missKernel);
    }

    public static Bitmap Thicken(this Bitmap img, Bitmap hitKernel, Bitmap missKernel)
    {
        return Morphology.Thicken(img, hitKernel, missKernel);
    }
    public static Bitmap Threshold(this Bitmap img, byte threshold, byte highValue = 255, byte lowValue = 0)
    {
        return Morphology.Threshold(img, threshold, highValue, lowValue);
    }
    public static Bitmap DistanceTransform(this Bitmap img)
    {
        return Morphology.DistanceTransform(img);
    }
}