namespace BlinkMore;

internal static class LaunchOptions
{
    public static bool Help { get; private set; }
    public static bool SelfTest { get; private set; }
    public static string? SettingsPath { get; private set; }
    public static string? ScreenshotDir { get; private set; }

    public static void Parse(string[] args)
    {
        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            switch (arg)
            {
                case "--help":
                case "-h":
                    Help = true;
                    break;
                case "--self-test":
                    SelfTest = true;
                    break;
                case "--settings" when i + 1 < args.Length:
                    SettingsPath = args[++i];
                    break;
                case "--screenshot-dir" when i + 1 < args.Length:
                    ScreenshotDir = args[++i];
                    break;
            }
        }
    }
}
