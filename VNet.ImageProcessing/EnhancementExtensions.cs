using System.Drawing;

namespace VNet.ImageProcessing;

public static class EnhancementExtensions
{
    public static Bitmap GaussianBlur(Bitmap source, int kernelSize, double sigma)
    {
        return Enhancement.GaussianBlur(source, kernelSize, sigma);
    }

    public static Bitmap Sharpen(Bitmap source, double strength)
    {
        return Enhancement.Sharpen(source, strength);
    }

    public static Bitmap AdjustBrightness(Bitmap source, int value)
    {
        return Enhancement.AdjustBrightness(source, value);
    }

    public static Bitmap AdjustContrast(Bitmap source, double contrastValue)
    {
        return Enhancement.AdjustContrast(source, contrastValue);
    }

    public static Bitmap AdjustGamma(Bitmap source, double gammaValue)
    {
        return Enhancement.AdjustGamma(source, gammaValue);
    }

    public static Bitmap EdgeEnhancement(Bitmap source)
    {
        return Enhancement.EdgeEnhancement(source);
    }

    public static Bitmap Convolve(Bitmap source, double[,] kernel)
    {
        return Enhancement.Convolve(source, kernel);
    }

    public static Bitmap Invert(Bitmap image)
    {
        return Enhancement.Invert(image);
    }

    public static Bitmap Difference(Bitmap input)
    {
        return Enhancement.Difference(input);
    }
}