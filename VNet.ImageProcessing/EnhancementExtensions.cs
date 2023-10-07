using System.Drawing;

namespace VNet.ImageProcessing;

public static class EnhancementExtensions
{
    public static Bitmap GaussianBlur(this Bitmap source, int kernelSize, double sigma)
    {
        return Enhancement.GaussianBlur(source, kernelSize, sigma);
    }

    public static Bitmap Sharpen(this Bitmap source, double strength)
    {
        return Enhancement.Sharpen(source, strength);
    }

    public static Bitmap AdjustBrightness(this Bitmap source, int value)
    {
        return Enhancement.AdjustBrightness(source, value);
    }

    public static Bitmap AdjustContrast(this Bitmap source, double contrastValue)
    {
        return Enhancement.AdjustContrast(source, contrastValue);
    }

    public static Bitmap AdjustGamma(this Bitmap source, double gammaValue)
    {
        return Enhancement.AdjustGamma(source, gammaValue);
    }

    public static Bitmap EdgeEnhancement(this Bitmap source)
    {
        return Enhancement.EdgeEnhancement(source);
    }

    public static Bitmap Convolve(this Bitmap source, double[,] kernel)
    {
        return Enhancement.Convolve(source, kernel);
    }

    public static Bitmap Invert(this Bitmap image)
    {
        return Enhancement.Invert(image);
    }

    public static Bitmap Difference(this Bitmap input)
    {
        return Enhancement.Difference(input);
    }
}