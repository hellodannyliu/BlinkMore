namespace BlinkMore.Core;

public enum SensitivityLevel
{
    Low = 0,
    Medium = 1,
    High = 2,
}

public static class SensitivityMap
{
    public static double ToValue(SensitivityLevel level) => level switch
    {
        SensitivityLevel.Low => AppConstants.MinSensitivity,
        SensitivityLevel.High => AppConstants.MaxSensitivity,
        _ => AppConstants.DefaultSensitivity,
    };

    public static SensitivityLevel FromValue(double value)
    {
        var range = AppConstants.MaxSensitivity - AppConstants.MinSensitivity;
        var step = range / 2.0;
        var relative = value - AppConstants.MinSensitivity;

        if (relative < step * 0.5)
            return SensitivityLevel.Low;
        if (relative < step * 1.5)
            return SensitivityLevel.Medium;
        return SensitivityLevel.High;
    }

    public static string LabelKey(SensitivityLevel level) => level switch
    {
        SensitivityLevel.Low => TextKey.Low,
        SensitivityLevel.High => TextKey.High,
        _ => TextKey.Med,
    };
}
