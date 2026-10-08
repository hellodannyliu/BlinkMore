using BlinkMore.Core;
using OpenCvSharp;

namespace BlinkMore.Vision;

internal static class LetterboxImage
{
    public static Mat Make(Mat source, Letterbox box, int size)
    {
        using var resized = new Mat();
        Cv2.Resize(source, resized, new Size(box.Width, box.Height));
        var canvas = new Mat(size, size, source.Type(), Scalar.All(0));
        using var region = new Mat(canvas, new Rect(box.X, box.Y, box.Width, box.Height));
        resized.CopyTo(region);
        return canvas;
    }
}