using System.Reflection;

namespace BlinkMore.Core;

public static class TextKey
{
    public const string SettingsTitle = "settings_title";
    public const string Tagline = "tagline";
    public const string Language = "language";
    public const string EyeTracking = "eye_tracking";
    public const string BlinkInterval = "blink_interval";
    public const string FadeDuration = "fade_duration";
    public const string BlinkSensitivity = "blink_sensitivity";
    public const string SensitivityHint = "sensitivity_hint";
    public const string FadeColor = "fade_color";
    public const string Camera = "camera";
    public const string PreviewFade = "preview_fade";
    public const string HowItWorks = "how_it_works";
    public const string Quit = "quit";
    public const string MadeBy = "made_by";
    public const string OpenSettings = "open_settings";
    public const string EnableTracking = "enable_tracking";
    public const string Low = "low";
    public const string Med = "med";
    public const string High = "high";
    public const string Seconds = "seconds";
    public const string NoCamera = "no_camera";
    public const string CameraName = "camera_name";
    public const string WelcomeTitle = "welcome_title";
    public const string WelcomeBody = "welcome_body";
    public const string CameraReason = "camera_reason";
    public const string ContinueCamera = "continue_camera";
    public const string Skip = "skip";
    public const string CameraRequiredTitle = "camera_required_title";
    public const string CameraRequiredBody = "camera_required_body";
    public const string OpenSystemSettings = "open_system_settings";
    public const string Cancel = "cancel";
    public const string CameraFailedTitle = "camera_failed_title";
    public const string CameraFailedBody = "camera_failed_body";
    public const string AlreadyRunning = "already_running";
    public const string Close = "close";
    public const string ColorBlack = "color_black";
    public const string ColorGray = "color_gray";
    public const string ColorWhite = "color_white";
    public const string ColorRed = "color_red";
    public const string ColorPurple = "color_purple";
    public const string ColorBlue = "color_blue";
    public const string ColorGreen = "color_green";
    public const string ColorYellow = "color_yellow";
    public const string ColorOrange = "color_orange";
    public const string HowIntro = "how_intro";
    public const string HowRequirements = "how_requirements";
    public const string HowReqOs = "how_req_os";
    public const string HowReqCamera = "how_req_camera";
    public const string HowPrivacy = "how_privacy";
    public const string HowPrivacyBody = "how_privacy_body";
    public const string HowTips = "how_tips";
    public const string HowTipGlasses = "how_tip_glasses";
    public const string HowTipAngle = "how_tip_angle";
    public const string HowTipPower = "how_tip_power";
    public const string HowLanguage = "how_language";
    public const string HowLanguageBody = "how_language_body";
    public const string HowTimeout = "how_timeout";
    public const string LangEn = "lang_en";
    public const string LangZh = "lang_zh";
    public const string VersionLabel = "version_label";
    public const string TrayTooltip = "tray_tooltip";

    public static IReadOnlyList<string> All { get; } = typeof(TextKey)
        .GetFields(BindingFlags.Public | BindingFlags.Static)
        .Where(field => field.IsLiteral && field.FieldType == typeof(string))
        .Select(field => (string)field.GetRawConstantValue()!)
        .ToArray();
}
