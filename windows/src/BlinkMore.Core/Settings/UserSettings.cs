namespace BlinkMore.Core;

public sealed class UserSettings
{
    public double FadeSpeedSeconds { get; set; } = AppConstants.DefaultFadeSpeed;
    public double BlinkThresholdSeconds { get; set; } = AppConstants.DefaultBlinkThreshold;
    public double Sensitivity { get; set; } = AppConstants.DefaultSensitivity;
    public string FadeColorHex { get; set; } = AppConstants.DefaultFadeColor;
    public bool EyeTrackingEnabled { get; set; }
    public bool HasShownOnboarding { get; set; }
    public string? SelectedCameraId { get; set; }
    public AppLanguage Language { get; set; } = AppLanguage.English;
    public Accelerator Accelerator { get; set; } = Accelerator.Cpu;

    public SensitivityLevel SensitivityLevel
    {
        get => SensitivityMap.FromValue(Sensitivity);
        set => Sensitivity = SensitivityMap.ToValue(value);
    }

    public static UserSettings CreateDefault(AppLanguage? language = null)
    {
        return new UserSettings
        {
            Language = language ?? AppLanguageExtensions.FromCulture(System.Globalization.CultureInfo.CurrentUICulture),
        };
    }

    public void Normalize()
    {
        FadeSpeedSeconds = Math.Clamp(
            Math.Round(FadeSpeedSeconds),
            AppConstants.MinFadeSpeed,
            AppConstants.MaxFadeSpeed);
        BlinkThresholdSeconds = Math.Clamp(
            Math.Round(BlinkThresholdSeconds),
            AppConstants.MinBlinkThreshold,
            AppConstants.MaxBlinkThreshold);
        Sensitivity = SensitivityMap.ToValue(SensitivityMap.FromValue(Sensitivity));
        FadeColorHex = AppConstants.NormalizeColor(FadeColorHex);
    }
}
