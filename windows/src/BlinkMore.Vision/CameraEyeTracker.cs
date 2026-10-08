using BlinkMore.Core;
using OpenCvSharp;

namespace BlinkMore.Vision;

public readonly record struct CameraDevice(string Id, int Index);

public sealed class CameraEyeTracker : IDisposable
{
    private readonly object _sync = new();
    private readonly EyeAnalyzer _analyzer;
    private CancellationTokenSource? _cts;
    private Task? _task;
    private bool _disposed;

    public CameraEyeTracker(bool useGpu = false)
        : this(EyeAnalyzer.Create(useGpu))
    {
    }

    public CameraEyeTracker(EyeAnalyzer analyzer)
    {
        _analyzer = analyzer;
    }

    public bool IsRunning
    {
        get
        {
            lock (_sync)
                return _task is { IsCompleted: false };
        }
    }

    public event Action<EyeSample>? Sampled;
    public event Action<string>? Failed;
    public event Action<CameraDevice>? Opened;

    public static IReadOnlyList<CameraDevice> Probe(int maxIndex = 3)
    {
        var cameras = new List<CameraDevice>();
        for (var index = 0; index < maxIndex; index++)
        {
            VideoCapture? capture = null;
            try
            {
                capture = new VideoCapture(index, Backend());
                if (capture.IsOpened())
                    cameras.Add(new CameraDevice(index.ToString(), index));
            }
            catch (OpenCVException)
            {
                // This index is not a usable camera.
            }
            finally
            {
                capture?.Release();
                capture?.Dispose();
            }
        }

        return cameras;
    }

    public void Start(string? cameraId, Func<SensitivityLevel> sensitivity)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        Stop();

        var cts = new CancellationTokenSource();
        var task = Task.Run(() => Loop(cameraId, sensitivity, cts.Token), cts.Token);
        lock (_sync)
        {
            _cts = cts;
            _task = task;
        }
    }

    public void Stop()
    {
        CancellationTokenSource? cts;
        Task? task;
        lock (_sync)
        {
            cts = _cts;
            task = _task;
            _cts = null;
            _task = null;
        }

        if (cts == null)
            return;

        try
        {
            cts.Cancel();
            task?.Wait(TimeSpan.FromSeconds(3));
        }
        catch (AggregateException)
        {
            // The capture loop already reported the failure.
        }
        finally
        {
            cts.Dispose();
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
        Stop();
        _analyzer.Dispose();
    }

    private void Loop(string? cameraId, Func<SensitivityLevel> sensitivity, CancellationToken token)
    {
        VideoCapture? capture = null;
        try
        {
            capture = Open(cameraId);
            if (capture == null || !capture.IsOpened())
            {
                Failed?.Invoke("open");
                return;
            }

            if (int.TryParse(cameraId, out var parsed))
                Opened?.Invoke(new CameraDevice(parsed.ToString(), parsed));
            else
                Opened?.Invoke(new CameraDevice("0", 0));

            using var frame = new Mat();
            var frameIndex = 0;
            while (!token.IsCancellationRequested)
            {
                if (!capture.Read(frame) || frame.Empty())
                {
                    Thread.Sleep(30);
                    continue;
                }

                frameIndex++;
                var interval = _analyzer.UsingGpu ? 1 : 2;
                if (frameIndex % interval != 0)
                    continue;

                var sample = _analyzer.Analyze(frame, sensitivity());
                Sampled?.Invoke(sample);
                Thread.Sleep(20);
            }
        }
        catch (Exception ex) when (ex is OpenCVException or ObjectDisposedException)
        {
            Failed?.Invoke(ex.Message);
        }
        finally
        {
            capture?.Release();
            capture?.Dispose();
        }
    }

    private static VideoCapture? Open(string? cameraId)
    {
        var index = 0;
        if (!string.IsNullOrWhiteSpace(cameraId) && !int.TryParse(cameraId, out index))
            index = 0;

        var capture = new VideoCapture(index, Backend());
        if (!capture.IsOpened() && index != 0)
        {
            capture.Dispose();
            capture = new VideoCapture(0, Backend());
        }

        if (!capture.IsOpened())
        {
            capture.Dispose();
            return null;
        }

        capture.Set(VideoCaptureProperties.FrameWidth, 640);
        capture.Set(VideoCaptureProperties.FrameHeight, 480);
        capture.Set(VideoCaptureProperties.BufferSize, 1);
        return capture;
    }

    private static VideoCaptureAPIs Backend()
        => OperatingSystem.IsWindows() ? VideoCaptureAPIs.DSHOW : VideoCaptureAPIs.ANY;
}
