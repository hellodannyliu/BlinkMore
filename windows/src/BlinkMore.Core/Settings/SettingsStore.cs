using System.Text.Json;
using System.Text.Json.Serialization;

namespace BlinkMore.Core;

public static class SettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    public static string DefaultPath()
    {
        var root = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        if (string.IsNullOrWhiteSpace(root))
        {
            root = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".config");
        }

        return Path.Combine(root, "BlinkMore", "settings.json");
    }

    public static UserSettings Load(string? path = null)
    {
        path ??= DefaultPath();
        try
        {
            if (!File.Exists(path))
                return UserSettings.CreateDefault();

            var json = File.ReadAllText(path);
            var file = JsonSerializer.Deserialize<SettingsFile>(json, JsonOptions);
            if (file == null)
                return UserSettings.CreateDefault();

            var settings = file.ToSettings();
            settings.Normalize();
            return settings;
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
        {
            return UserSettings.CreateDefault();
        }
    }

    public static void Save(UserSettings settings, string? path = null)
    {
        path ??= DefaultPath();
        settings.Normalize();
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        var json = JsonSerializer.Serialize(SettingsFile.From(settings), JsonOptions);
        var temporary = path + ".tmp";
        File.WriteAllText(temporary, json);
        File.Move(temporary, path, overwrite: true);
    }

    private sealed class SettingsFile
    {
        public double FadeSpeedSeconds { get; set; } = AppConstants.DefaultFadeSpeed;
        public double BlinkThresholdSeconds { get; set; } = AppConstants.DefaultBlinkThreshold;
        public double Sensitivity { get; set; } = AppConstants.DefaultSensitivity;
        public string FadeColorHex { get; set; } = AppConstants.DefaultFadeColor;
        public bool EyeTrackingEnabled { get; set; }
        public bool HasShownOnboarding { get; set; }
        public string? SelectedCameraId { get; set; }
        public string Language { get; set; } = "en";

        public UserSettings ToSettings() => new()
        {
            FadeSpeedSeconds = FadeSpeedSeconds,
            BlinkThresholdSeconds = BlinkThresholdSeconds,
            Sensitivity = Sensitivity,
            FadeColorHex = FadeColorHex,
            EyeTrackingEnabled = EyeTrackingEnabled,
            HasShownOnboarding = HasShownOnboarding,
            SelectedCameraId = SelectedCameraId,
            Language = AppLanguageExtensions.FromCode(Language),
        };

        public static SettingsFile From(UserSettings settings) => new()
        {
            FadeSpeedSeconds = settings.FadeSpeedSeconds,
            BlinkThresholdSeconds = settings.BlinkThresholdSeconds,
            Sensitivity = settings.Sensitivity,
            FadeColorHex = settings.FadeColorHex,
            EyeTrackingEnabled = settings.EyeTrackingEnabled,
            HasShownOnboarding = settings.HasShownOnboarding,
            SelectedCameraId = settings.SelectedCameraId,
            Language = settings.Language.ToCode(),
        };
    }
}
