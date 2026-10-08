using System.Globalization;

namespace BlinkMore.Core;

public enum AppLanguage
{
    English,
    Chinese,
}

public static class AppLanguageExtensions
{
    public static string ToCode(this AppLanguage language)
        => language == AppLanguage.Chinese ? "zh" : "en";

    public static AppLanguage FromCode(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return FromCulture(CultureInfo.CurrentUICulture);

        return code.StartsWith("zh", StringComparison.OrdinalIgnoreCase)
            ? AppLanguage.Chinese
            : AppLanguage.English;
    }

    public static AppLanguage FromCulture(CultureInfo culture)
    {
        var name = culture.Name;
        if (name.StartsWith("zh", StringComparison.OrdinalIgnoreCase))
            return AppLanguage.Chinese;

        return AppLanguage.English;
    }
}
