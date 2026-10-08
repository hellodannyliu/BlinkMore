namespace BlinkMore.Core;

public readonly record struct FadeColorOption(string Key, string Hex);

public static class AppConstants
{
    public const double DefaultFadeSpeed = 5;
    public const double MinFadeSpeed = 1;
    public const double MaxFadeSpeed = 5;

    public const double DefaultBlinkThreshold = 6;
    public const double MinBlinkThreshold = 3;
    public const double MaxBlinkThreshold = 12;

    public const double DefaultSensitivity = 0.16;
    public const double MinSensitivity = 0.10;
    public const double MaxSensitivity = 0.22;

    public const double FadeTimeoutSeconds = 6;
    public const double FadeOutMilliseconds = 50;

    public const string DefaultFadeColor = "#000000";
    public const string AuthorUrl = "https://github.com/oxremy";

    public static readonly FadeColorOption[] FadeColors =
    [
        new("color_black", "#000000"),
        new("color_gray", "#808080"),
        new("color_white", "#FFFFFF"),
        new("color_red", "#E63333"),
        new("color_purple", "#9933CC"),
        new("color_blue", "#3366E6"),
        new("color_green", "#33B34D"),
        new("color_yellow", "#E6CC33"),
        new("color_orange", "#E6801A"),
    ];

    public static string NormalizeColor(string? hex)
    {
        if (!TryParseHex(hex, out var r, out var g, out var b))
            return DefaultFadeColor;

        var canonical = $"#{r:X2}{g:X2}{b:X2}";
        foreach (var color in FadeColors)
        {
            if (string.Equals(color.Hex, canonical, StringComparison.OrdinalIgnoreCase))
                return color.Hex;
        }

        return canonical;
    }

    public static bool TryParseHex(string? hex, out byte r, out byte g, out byte b)
    {
        r = g = b = 0;
        if (string.IsNullOrWhiteSpace(hex))
            return false;

        var text = hex.Trim();
        if (text.StartsWith('#'))
            text = text[1..];

        if (text.Length == 3)
        {
            text = string.Concat(text[0], text[0], text[1], text[1], text[2], text[2]);
        }

        if (text.Length != 6)
            return false;

        if (!byte.TryParse(text[..2], System.Globalization.NumberStyles.HexNumber, null, out r)
            || !byte.TryParse(text[2..4], System.Globalization.NumberStyles.HexNumber, null, out g)
            || !byte.TryParse(text[4..6], System.Globalization.NumberStyles.HexNumber, null, out b))
        {
            return false;
        }

        return true;
    }
}
