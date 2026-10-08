using System.ComponentModel;
using Avalonia.Media;
using BlinkMore.Core;

namespace BlinkMore;

internal abstract class LocalizedViewModel : INotifyPropertyChanged
{
    protected LocalizedViewModel(AppController controller) => Controller = controller;

    protected AppController Controller { get; }

    public event PropertyChangedEventHandler? PropertyChanged;

    public void Refresh() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));

    protected string T(string key) => Controller.Loc[key];

    protected void Raise(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

internal sealed class ColorOption
{
    public required string Hex { get; init; }
    public required string Name { get; init; }
    public bool IsSelected { get; init; }
    public IBrush Swatch { get; init; } = Brushes.Black;
}

internal sealed class CameraOption
{
    public required string Id { get; init; }
    public required string Name { get; init; }
}

internal sealed class SettingsViewModel : LocalizedViewModel
{
    public SettingsViewModel(AppController controller) : base(controller)
    {
    }

    public string Title => T(TextKey.SettingsTitle);
    public string Tagline => T(TextKey.Tagline);
    public string LanguageLabel => T(TextKey.Language);
    public string EyeTrackingLabel => T(TextKey.EyeTracking);
    public string BlinkIntervalLabel => T(TextKey.BlinkInterval);
    public string FadeDurationLabel => T(TextKey.FadeDuration);
    public string SensitivityLabel => T(TextKey.BlinkSensitivity);
    public string SensitivityHint => T(TextKey.SensitivityHint);
    public string FadeColorLabel => T(TextKey.FadeColor);
    public string CameraLabel => T(TextKey.Camera);
    public string PreviewLabel => T(TextKey.PreviewFade);
    public string HowItWorksLabel => T(TextKey.HowItWorks);
    public string MadeByLabel => T(TextKey.MadeBy);
    public string NoCameraText => T(TextKey.NoCamera);
    public string VersionText => Controller.Loc.Format(TextKey.VersionLabel, Controller.Version);
    public bool IsEnglish => Controller.Settings.Language == AppLanguage.English;
    public bool IsChinese => Controller.Settings.Language == AppLanguage.Chinese;
    public bool HasCameras => Cameras.Count > 0;

    public string BlinkValueText => Controller.Loc.Format(TextKey.Seconds, (int)Controller.Settings.BlinkThresholdSeconds);
    public string FadeValueText => Controller.Loc.Format(TextKey.Seconds, (int)Controller.Settings.FadeSpeedSeconds);
    public string SensitivityValueText => T(SensitivityMap.LabelKey(Controller.Settings.SensitivityLevel));

    public double BlinkThreshold
    {
        get => Controller.Settings.BlinkThresholdSeconds;
        set
        {
            if (Math.Abs(Controller.Settings.BlinkThresholdSeconds - value) < 0.01)
                return;
            Controller.SetBlinkThreshold(value);
            Raise(nameof(BlinkValueText));
        }
    }

    public double FadeSpeed
    {
        get => Controller.Settings.FadeSpeedSeconds;
        set
        {
            if (Math.Abs(Controller.Settings.FadeSpeedSeconds - value) < 0.01)
                return;
            Controller.SetFadeSpeed(value);
            Raise(nameof(FadeValueText));
        }
    }

    public double SensitivityIndex
    {
        get => (int)Controller.Settings.SensitivityLevel;
        set
        {
            var level = (SensitivityLevel)Math.Clamp((int)Math.Round(value), 0, 2);
            if (level == Controller.Settings.SensitivityLevel)
                return;
            Controller.SetSensitivity(level);
            Raise(nameof(SensitivityValueText));
            Raise(nameof(SensitivityIndex));
        }
    }

    public bool IsEyeTracking
    {
        get => Controller.Settings.EyeTrackingEnabled;
        set
        {
            if (value == Controller.Settings.EyeTrackingEnabled)
                return;
            _ = Controller.SetEyeTrackingAsync(value);
        }
    }

    public IReadOnlyList<ColorOption> Colors { get; private set; } = [];
    public IReadOnlyList<CameraOption> Cameras { get; private set; } = [];
    public CameraOption? SelectedCamera =>
        Cameras.FirstOrDefault(camera => camera.Id == Controller.Settings.SelectedCameraId)
        ?? Cameras.FirstOrDefault();

    public void Rebuild()
    {
        Colors = AppConstants.FadeColors.Select(color => new ColorOption
        {
            Hex = color.Hex,
            Name = T(color.Key),
            IsSelected = string.Equals(color.Hex, Controller.Settings.FadeColorHex, StringComparison.OrdinalIgnoreCase),
            Swatch = new SolidColorBrush(Color.Parse(color.Hex)),
        }).ToArray();

        Cameras = Controller.Cameras.Select(camera => new CameraOption
        {
            Id = camera.Id,
            Name = Controller.Loc.Format(TextKey.CameraName, camera.Index + 1),
        }).ToArray();

        Refresh();
    }
}

internal sealed class HowItWorksViewModel : LocalizedViewModel
{
    public HowItWorksViewModel(AppController controller) : base(controller)
    {
    }

    public string Title => T(TextKey.HowItWorks);
    public string Intro => T(TextKey.HowIntro);
    public string Requirements => T(TextKey.HowRequirements);
    public string ReqOs => T(TextKey.HowReqOs);
    public string ReqCamera => T(TextKey.HowReqCamera);
    public string Privacy => T(TextKey.HowPrivacy);
    public string PrivacyBody => T(TextKey.HowPrivacyBody);
    public string Tips => T(TextKey.HowTips);
    public string TipGlasses => T(TextKey.HowTipGlasses);
    public string TipAngle => T(TextKey.HowTipAngle);
    public string TipPower => T(TextKey.HowTipPower);
    public string Language => T(TextKey.HowLanguage);
    public string LanguageBody => T(TextKey.HowLanguageBody);
    public string Timeout => T(TextKey.HowTimeout);
    public string CloseLabel => T(TextKey.Close);
}

internal sealed class OnboardingViewModel : LocalizedViewModel
{
    public OnboardingViewModel(AppController controller) : base(controller)
    {
    }

    public string Title => T(TextKey.WelcomeTitle);
    public string Body => T(TextKey.WelcomeBody);
    public string Reason => T(TextKey.CameraReason);
    public string ContinueLabel => T(TextKey.ContinueCamera);
    public string SkipLabel => T(TextKey.Skip);
}
