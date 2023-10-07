using System.Drawing;

namespace VNet.ImageProcessing;

public static class AlgebraExtensions
{
    public static Bitmap Add(this Bitmap img1, Bitmap img2)
    {
        return Algebra.Add(img1, img2);
    }

    public static Bitmap Subtract(this Bitmap img1, Bitmap img2)
    {
        return Algebra.Subtract(img1, img2);
    }

    public static Bitmap Multiply(this Bitmap img1, Bitmap img2)
    {
        return Algebra.Multiply(img1, img2);
    }

    public static Bitmap Divide(this Bitmap img1, Bitmap img2)
    {
        return Algebra.Divide(img1, img2);
    }

    public static Bitmap Union(this Bitmap img1, Bitmap img2)
    {
        return Algebra.Union(img1, img2);
    }

    public static Bitmap BitwiseAnd(this Bitmap img1, Bitmap img2)
    {
        return Algebra.BitwiseAnd(img1, img2);
    }

    public static Bitmap BitwiseOr(this Bitmap img1, Bitmap img2)
    {
        return Algebra.BitwiseOr(img1, img2);
    }

    public static Bitmap BitwiseXor(this Bitmap img1, Bitmap img2)
    {
        return Algebra.BitwiseXor(img1, img2);
    }

    public static Bitmap BitwiseNot(this Bitmap img)
    {
        return Algebra.BitwiseNot(img);
    }

    public static Bitmap Intersect(this Bitmap image1, Bitmap image2, byte threshold)
    {
        return Algebra.Intersect(image1, image2, threshold);
    }

    public static bool AreEqual(this Bitmap img1, Bitmap img2)
    {
        return Algebra.AreEqual(img1, img2);
    }
}