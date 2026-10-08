using BlinkMore.Core;
using OpenCvSharp;
using OpenCvSharp.Dnn;

namespace BlinkMore.Vision;

public sealed class YunetFaceDetector : IDisposable
{
    public const int InputSize = 320;

    private readonly FaceDetectorYN _detector;

    private YunetFaceDetector(FaceDetectorYN detector, string deviceName)
    {
        _detector = detector;
        DeviceName = deviceName;
    }

    public string DeviceName { get; }

    public static YunetFaceDetector CreateCpu()
    {
        var detector = Create(Target.CPU);
        detector.Warmup();
        return detector;
    }

    public static YunetFaceDetector CreateForIntelGpu(OpenClGpu gpu)
    {
        Environment.SetEnvironmentVariable("OPENCV_OPENCL_DEVICE", IntelGraphics.OpenCvDeviceVariable(gpu));
        Exception? last = null;
        foreach (var target in new[] { Target.OPENCL_FP16, Target.OPENCL })
        {
            YunetFaceDetector? detector = null;
            try
            {
                detector = Create(target);
                detector.Warmup();
                return detector;
            }
            catch (Exception ex) when (ex is OpenCVException or EntryPointNotFoundException or DllNotFoundException)
            {
                detector?.Dispose();
                last = ex;
            }
        }

        throw last ?? new InvalidOperationException("Intel graphics did not start.");
    }

    public List<Rect> Detect(Mat bgrSquare)
    {
        using var faces = new Mat();
        _detector.Detect(bgrSquare, faces);
        var found = new List<Rect>();
        if (faces.Empty() || faces.Cols < 4)
            return found;

        var stride = faces.Cols >= 15 ? 15 : faces.Cols;
        var count = faces.Cols == stride ? faces.Rows : faces.Cols / stride;
        if (faces.Rows > 1)
            count = faces.Rows;

        for (var i = 0; i < count; i++)
        {
            var x = Read(faces, i, 0, stride);
            var y = Read(faces, i, 1, stride);
            var width = Read(faces, i, 2, stride);
            var height = Read(faces, i, 3, stride);
            if (width < 2 || height < 2)
                continue;
            found.Add(new Rect((int)Math.Round(x), (int)Math.Round(y), (int)Math.Round(width), (int)Math.Round(height)));
        }

        return found;
    }

    public void Dispose() => _detector.Dispose();

    private static YunetFaceDetector Create(Target target)
    {
        var model = ModelFiles.EnsureYunet();
        var detector = FaceDetectorYN.Create(
            model,
            "",
            new Size(InputSize, InputSize),
            0.6f,
            0.3f,
            5000,
            Backend.OPENCV,
            target);
        return new YunetFaceDetector(detector, target == Target.CPU ? "CPU" : "Intel GPU");
    }

    private void Warmup()
    {
        using var blank = new Mat(InputSize, InputSize, MatType.CV_8UC3, Scalar.All(0));
        Detect(blank);
    }

    private static float Read(Mat faces, int row, int column, int stride)
    {
        if (faces.Rows > 1 || faces.Cols == stride)
            return faces.At<float>(row, column);
        return faces.At<float>(0, row * stride + column);
    }
}
