using BlinkMore.Core;
using OpenCvSharp;

namespace BlinkMore.Vision;

public sealed class EyeAnalyzer : IDisposable
{
    private readonly CascadeClassifier _face;
    private readonly CascadeClassifier _eye;
    private readonly CascadeClassifier _glasses;
    private readonly YunetFaceDetector? _gpu;

    public EyeAnalyzer(CascadePaths paths)
        : this(paths, gpu: null)
    {
    }

    private EyeAnalyzer(CascadePaths paths, YunetFaceDetector? gpu)
    {
        _face = Load(paths.Face);
        _eye = Load(paths.Eye);
        _glasses = Load(paths.Glasses);
        _gpu = gpu;
    }

    public bool UsingGpu => _gpu != null;

    public static EyeAnalyzer Create(bool useGpu)
    {
        var paths = CascadeFiles.Extract();
        if (!useGpu)
            return new EyeAnalyzer(paths);

        var gpu = IntelGraphics.Prefer(OpenClCatalog.List());
        if (gpu == null)
            return new EyeAnalyzer(paths);

        try
        {
            return new EyeAnalyzer(paths, YunetFaceDetector.CreateForIntelGpu(gpu));
        }
        catch (Exception)
        {
            return new EyeAnalyzer(paths);
        }
    }

    public EyeSample Analyze(Mat bgr, SensitivityLevel sensitivity)
    {
        if (bgr.Empty())
            return new EyeSample(0, 0);

        using var small = Downscale(bgr);
        using var gray = new Mat();
        Cv2.CvtColor(small, gray, ColorConversionCodes.BGR2GRAY);
        using var equalized = new Mat();
        Cv2.EqualizeHist(gray, equalized);

        if (_gpu != null)
            return AnalyzeOnGpu(small, equalized, sensitivity);

        var faces = _face.DetectMultiScale(
            equalized,
            1.1,
            5,
            HaarDetectionTypes.ScaleImage,
            new Size(70, 70));

        if (faces.Length != 1)
            return new EyeSample(faces.Length, 0);

        return CountEyes(equalized, faces[0], sensitivity);
    }

    public void Dispose()
    {
        _gpu?.Dispose();
        _face.Dispose();
        _eye.Dispose();
        _glasses.Dispose();
    }

    private EyeSample AnalyzeOnGpu(Mat small, Mat gray, SensitivityLevel sensitivity)
    {
        var box = Letterbox.Fit(small.Width, small.Height, YunetFaceDetector.InputSize);
        using var canvas = LetterboxImage.Make(small, box, YunetFaceDetector.InputSize);
        var faces = _gpu!.Detect(canvas);
        if (faces.Count != 1)
            return new EyeSample(faces.Count, 0);

        var face = faces[0];
        if (!box.TryMapToSource(face.X, face.Y, face.Width, face.Height, small.Width, small.Height, out var x, out var y, out var width, out var height))
            return new EyeSample(1, 0);

        return CountEyes(gray, new Rect(x, y, width, height), sensitivity);
    }

    private EyeSample CountEyes(Mat gray, Rect face, SensitivityLevel sensitivity)
    {
        var roi = new Rect(face.X, face.Y, face.Width, Math.Max(1, (int)(face.Height * 0.62)));
        roi = Clamp(roi, gray.Width, gray.Height);
        if (roi.Width < 20 || roi.Height < 20)
            return new EyeSample(1, 0);

        using var faceRegion = new Mat(gray, roi);
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

    private static Mat Downscale(Mat bgr)
    {
        if (bgr.Width <= 640)
            return bgr.Clone();

        var scale = 640.0 / bgr.Width;
        var resized = new Mat();
        Cv2.Resize(bgr, resized, new Size(640, Math.Max(1, (int)(bgr.Height * scale))));
        return resized;
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
