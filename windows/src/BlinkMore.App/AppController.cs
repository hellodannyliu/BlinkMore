using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Avalonia.Threading;
using BlinkMore.Core;
using BlinkMore.Vision;

namespace BlinkMore;

internal sealed class AppController : IDisposable
{
    private readonly string? _settingsPath;
    private readonly FadeMonitor _monitor = new();
    private readonly EyeStateStabilizer _stabilizer = new();
    private readonly object _sampleGate = new();
    private CameraEyeTracker? _tracker;
    private IReadOnlyList<CameraDevice> _cameras = [];
    private int _sampleQueued;
    private EyeSample _latestSample;
    private int _previewToken;
    private bool _eyesOpen;
    private bool _disposed;

    public AppController(UserSettings settings, LocalizationService localization, string? settingsPath)
    {
        Settings = settings;
        Loc = localization;
        _settingsPath = settingsPath;
        Overlay = new FadeOverlay();
        _monitor.BlinkThreshold = TimeSpan.FromSeconds(settings.BlinkThresholdSeconds);
        _monitor.FadeTimeout = TimeSpan.FromSeconds(AppConstants.FadeTimeoutSeconds);
        GpuReport = settings.Accelerator == Accelerator.Gpu
            ? GpuSupport.Probe()
            : AcceleratorReport.Cpu();
    }

    public UserSettings Settings { get; }
    public LocalizationService Loc { get; }
    public FadeOverlay Overlay { get; }
    public Window? DialogOwner { get; set; }
    public bool EyesOpen => _eyesOpen;
    public IReadOnlyList<CameraDevice> Cameras => _cameras;
    public string Version { get; } = typeof(AppController).Assembly.GetName().Version?.ToString(3) ?? "1.1.0";
    public AcceleratorReport GpuReport { get; private set; }

    public event Action? DisplayChanged;
    public event Action? TrackingChanged;
    public event Action? EyeChanged;
    public event Action? CamerasChanged;

    public Action? ShowSettings { get; set; }
    public Action? ShowHowItWorks { get; set; }

    public void SetLanguage(AppLanguage language)
    {
        if (Settings.Language == language && Loc.Language == language)
            return;

        Settings.Language = language;
        Loc.SetLanguage(language);
        Save();
        DisplayChanged?.Invoke();
    }

    public void SetBlinkThreshold(double seconds)
    {
        Settings.BlinkThresholdSeconds = seconds;
        Settings.Normalize();
        _monitor.BlinkThreshold = TimeSpan.FromSeconds(Settings.BlinkThresholdSeconds);
        Save();
        DisplayChanged?.Invoke();
    }

    public void SetFadeSpeed(double seconds)
    {
        Settings.FadeSpeedSeconds = seconds;
        Settings.Normalize();
        Save();
        DisplayChanged?.Invoke();
    }

    public void SetSensitivity(SensitivityLevel level)
    {
        if (Settings.SensitivityLevel == level)
            return;
        Settings.SensitivityLevel = level;
        Save();
        DisplayChanged?.Invoke();
    }

    public void SetFadeColor(string hex)
    {
        Settings.FadeColorHex = AppConstants.NormalizeColor(hex);
        Save();
        if (_monitor.IsFaded)
            Overlay.Apply(ParseColor(), TimeSpan.Zero, immediate: true);
        DisplayChanged?.Invoke();
    }

    public void SetAccelerator(Accelerator accelerator)
    {
        Settings.Accelerator = accelerator;
        Save();
        GpuReport = accelerator == Accelerator.Gpu
            ? GpuSupport.Probe()
            : AcceleratorReport.Cpu();
        Log.Info(GpuReport.UsingGpu
            ? "Face detection on " + GpuReport.DeviceName
            : "Face detection on CPU. " + (GpuReport.FailureDetail ?? ""));
        RestartTrackerIfRunning();
        DisplayChanged?.Invoke();
    }

    public void SetCamera(string id)
    {
        if (Settings.SelectedCameraId == id)
            return;

        Settings.SelectedCameraId = id;
        Save();
        if (Settings.EyeTrackingEnabled)
            Tracker.Start(id, () => Settings.SensitivityLevel);
    }

    public async Task SetEyeTrackingAsync(bool enabled)
    {
        if (enabled)
        {
            Settings.EyeTrackingEnabled = true;
            Save();
            _monitor.EyeTrackingEnabled = true;
            _stabilizer.Reset();
            try
            {
                Tracker.Start(Settings.SelectedCameraId, () => Settings.SensitivityLevel);
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                Settings.EyeTrackingEnabled = false;
                Save();
                TrackingChanged?.Invoke();
                await ShowCameraFailureAsync();
                return;
            }

            TrackingChanged?.Invoke();
            return;
        }

        Settings.EyeTrackingEnabled = false;
        Save();
        StopTracking();
        TrackingChanged?.Invoke();
    }

    public async Task CompleteOnboardingAsync(bool enableTracking)
    {
        if (!Settings.HasShownOnboarding)
        {
            Settings.HasShownOnboarding = true;
            Save();
        }

        await SetEyeTrackingAsync(enableTracking);
        DisplayChanged?.Invoke();
    }

    public void PreviewFade()
    {
        var token = Interlocked.Increment(ref _previewToken);
        var duration = TimeSpan.FromSeconds(Settings.FadeSpeedSeconds);
        Overlay.Apply(ParseColor(), duration, immediate: false);
        _ = Task.Run(async () =>
        {
            await Task.Delay(duration + TimeSpan.FromMilliseconds(700));
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (token != _previewToken || _monitor.IsFaded)
                    return;
                Overlay.Remove(TimeSpan.FromMilliseconds(AppConstants.FadeOutMilliseconds));
            });
        });
    }

    public void RefreshCameras()
    {
        if (LaunchOptions.SelfTest || (_tracker?.IsRunning ?? false))
            return;

        Task.Run(() =>
        {
            try
            {
                var cameras = CameraEyeTracker.Probe();
                Dispatcher.UIThread.Post(() =>
                {
                    _cameras = cameras;
                    CamerasChanged?.Invoke();
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        });
    }

    public void Quit()
    {
        Dispose();
        if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.Shutdown(0);
    }

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
        StopTracking();
        Overlay.Dispose();
    }

    private CameraEyeTracker Tracker => _tracker ??= CreateTracker();

    private void RestartTrackerIfRunning()
    {
        var running = _tracker?.IsRunning ?? false;
        _tracker?.Dispose();
        _tracker = null;
        if (running && Settings.EyeTrackingEnabled)
            Tracker.Start(Settings.SelectedCameraId, () => Settings.SensitivityLevel);
    }

    private CameraEyeTracker CreateTracker()
    {
        var useGpu = Settings.Accelerator == Accelerator.Gpu && GpuReport.UsingGpu;
        var tracker = new CameraEyeTracker(useGpu);
        tracker.Sampled += sample =>
        {
            lock (_sampleGate)
                _latestSample = sample;
            if (Interlocked.Exchange(ref _sampleQueued, 1) == 0)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    EyeSample sampleToHandle;
                    lock (_sampleGate)
                        sampleToHandle = _latestSample;
                    Interlocked.Exchange(ref _sampleQueued, 0);
                    OnSample(sampleToHandle);
                });
            }
        };
        tracker.Failed += reason => Dispatcher.UIThread.Post(() => OnFailed(reason));
        tracker.Opened += device => Dispatcher.UIThread.Post(() => OnOpened(device));
        return tracker;
    }

    private void OnSample(EyeSample sample)
    {
        if (!Settings.EyeTrackingEnabled || _disposed)
            return;

        var observation = BlinkClassifier.Classify(sample, Settings.SensitivityLevel);
        var stable = _stabilizer.Push(observation);
        _monitor.BlinkThreshold = TimeSpan.FromSeconds(Settings.BlinkThresholdSeconds);
        _monitor.FadeTimeout = TimeSpan.FromSeconds(AppConstants.FadeTimeoutSeconds);
        _monitor.EyeTrackingEnabled = true;
        var decision = _monitor.Tick(DateTime.UtcNow, stable);

        var open = stable == EyeObservation.Open;
        if (open != _eyesOpen)
        {
            _eyesOpen = open;
            EyeChanged?.Invoke();
        }

        ApplyDecision(decision);
    }

    private void ApplyDecision(FadeDecision decision)
    {
        switch (decision.Transition)
        {
            case FadeTransition.Applied:
                Interlocked.Increment(ref _previewToken);
                Overlay.Apply(ParseColor(), TimeSpan.FromSeconds(Settings.FadeSpeedSeconds), immediate: false);
                break;
            case FadeTransition.Removed:
                Overlay.Remove(TimeSpan.FromMilliseconds(AppConstants.FadeOutMilliseconds));
                break;
            case FadeTransition.TimedOut:
                Overlay.Remove(TimeSpan.FromMilliseconds(AppConstants.FadeOutMilliseconds));
                Settings.EyeTrackingEnabled = false;
                Save();
                StopTracking();
                TrackingChanged?.Invoke();
                break;
        }
    }

    private void OnFailed(string reason)
    {
        Log.Info("Camera failed: " + reason);
        if (!Settings.EyeTrackingEnabled)
            return;

        Settings.EyeTrackingEnabled = false;
        Save();
        StopTracking();
        TrackingChanged?.Invoke();
        _ = ShowCameraFailureAsync();
    }

    private void OnOpened(CameraDevice device)
    {
        var list = _cameras.ToList();
        if (list.All(camera => camera.Id != device.Id))
        {
            list.Add(device);
            _cameras = list;
            CamerasChanged?.Invoke();
        }

        if (Settings.SelectedCameraId != device.Id)
        {
            Settings.SelectedCameraId = device.Id;
            Save();
        }
    }

    private void StopTracking()
    {
        _tracker?.Stop();
        _monitor.Reset();
        _stabilizer.Reset();
        _eyesOpen = false;
        Overlay.Remove(TimeSpan.FromMilliseconds(AppConstants.FadeOutMilliseconds));
        EyeChanged?.Invoke();
    }

    private async Task ShowCameraFailureAsync()
    {
        if (LaunchOptions.SelfTest)
            return;

        var open = await ConfirmDialog.ShowAsync(
            DialogOwner,
            Loc[TextKey.CameraFailedTitle],
            Loc[TextKey.CameraFailedBody],
            Loc[TextKey.OpenSystemSettings],
            Loc[TextKey.Close]);
        if (open)
            SystemSettings.OpenCameraPrivacy();
    }

    private Color ParseColor() => Color.Parse(Settings.FadeColorHex);

    private void Save() => SettingsStore.Save(Settings, _settingsPath);
}
