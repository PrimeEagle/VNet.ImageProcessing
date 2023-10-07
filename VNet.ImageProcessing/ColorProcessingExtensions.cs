using System.Drawing;

namespace VNet.ImageProcessing;

public static class ColorProcessingExtensions
{
    public static Bitmap ConvertToGrayscale(this Bitmap sourceBitmap, double redWeight, double greenWeight, double blueWeight)
    {
        return ColorProcessing.ConvertToGrayscale(sourceBitmap, redWeight, greenWeight, blueWeight);
    }

    public static Bitmap ColorCorrection(Bitmap sourceBitmap, double redMultiplier, double greenMultiplier, double blueMultiplier)
    {
        return ColorProcessing.ColorCorrection(sourceBitmap, redMultiplier, greenMultiplier, blueMultiplier);
    }
}