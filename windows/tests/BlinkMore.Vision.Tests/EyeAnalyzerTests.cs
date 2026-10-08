using BlinkMore.Core;
using Xunit;
using OpenCvSharp;

namespace BlinkMore.Vision.Tests;

public class EyeAnalyzerTests : IDisposable
{
    private readonly EyeAnalyzer _analyzer = new(CascadeFiles.Extract());

    [Fact]
    public void BlankFrameHasNoFace()
    {
        using var frame = new Mat(480, 640, MatType.CV_8UC3, Scalar.All(0));
        var sample = _analyzer.Analyze(frame, SensitivityLevel.Medium);
        Assert.Equal(0, sample.FaceCount);
        Assert.Equal(EyeObservation.Absent, BlinkClassifier.Classify(sample, SensitivityLevel.Medium));
    }

    [Fact]
    public void EmptyFrameDoesNotThrow()
    {
        using var frame = new Mat();
        var sample = _analyzer.Analyze(frame, SensitivityLevel.High);
        Assert.Equal(0, sample.FaceCount);
    }

    public void Dispose() => _analyzer.Dispose();
}

public class YuNetModelTests
{
    [Fact]
    public void CpuModelRejectsABlankFrame()
    {
        using var detector = YunetFaceDetector.CreateCpu();
        using var frame = new Mat(YunetFaceDetector.InputSize, YunetFaceDetector.InputSize, MatType.CV_8UC3, Scalar.All(0));
        Assert.Empty(detector.Detect(frame));
    }
}
