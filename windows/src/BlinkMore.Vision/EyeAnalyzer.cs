using BlinkMore.Core;
using OpenCvSharp;

namespace BlinkMore.Vision;

public sealed class EyeAnalyzer : IDisposable
{
    private readonly CascadeClassifier _face;
    private readonly CascadeClassifier _eye;
    private readonly CascadeClassifier _glasses;

    public EyeAnalyzer(CascadePaths paths)
    {
        _face = Load(paths.Face);
        _eye = Load(paths.Eye);
        _glasses = Load(paths.Glasses);
    }

    public EyeSample Analyze(Mat bgr, SensitivityLevel sensitivity)
    {
        if (bgr.Empty())
            return new EyeSample(0, 0);

        using var gray = new Mat();
        Cv2.CvtColor(bgr, gray, ColorConversionCodes.BGR2GRAY);
        if (gray.Width > 640)
        {
            var scale = 640.0 / gray.Width;
            Cv2.Resize(gray, gray, new Size(640, Math.Max(1, (int)(gray.Height * scale))));
        }

        using var equalized = new Mat();
        Cv2.EqualizeHist(gray, equalized);

        var faces = _face.DetectMultiScale(
            equalized,
            1.1,
            5,
            HaarDetectionTypes.ScaleImage,
            new Size(70, 70));

        if (faces.Length != 1)
            return new EyeSample(faces.Length, 0);

        var face = faces[0];
        var roi = new Rect(face.X, face.Y, face.Width, Math.Max(1, (int)(face.Height * 0.62)));
        roi = Clamp(roi, equalized.Width, equalized.Height);
        if (roi.Width < 20 || roi.Height < 20)
            return new EyeSample(1, 0);

        using var faceRegion = new Mat(equalized, roi);
        var tuning = BlinkClassifier.TuningFor(sensitivity);
        var minimum = new Size(tuning.MinEyeSize, tuning.MinEyeSize);
        var eyes = _eye.DetectMultiScale(
            faceRegion,
            tuning.ScaleFactor,
            tuning.MinNeighbors,
            HaarDetectionTypes.ScaleImage,
            minimum);
        var glasses = _glasses.DetectMultiScale(
            faceRegion,
            tuning.ScaleFactor,
            tuning.MinNeighbors,
            HaarDetectionTypes.ScaleImage,
            minimum);
        var count = Math.Min(2, Math.Max(eyes.Length, glasses.Length));
        return new EyeSample(1, count);
    }

    public void Dispose()
    {
        _face.Dispose();
        _eye.Dispose();
        _glasses.Dispose();
    }

    private static CascadeClassifier Load(string path)
    {
        var classifier = new CascadeClassifier(path);
        if (classifier.Empty())
            throw new InvalidOperationException($"Could not load cascade '{path}'.");
        return classifier;
    }

    private static Rect Clamp(Rect rect, int width, int height)
    {
        var x = Math.Clamp(rect.X, 0, Math.Max(0, width - 1));
        var y = Math.Clamp(rect.Y, 0, Math.Max(0, height - 1));
        var right = Math.Clamp(rect.Right, x + 1, width);
        var bottom = Math.Clamp(rect.Bottom, y + 1, height);
        return new Rect(x, y, Math.Max(1, right - x), Math.Max(1, bottom - y));
    }
}
