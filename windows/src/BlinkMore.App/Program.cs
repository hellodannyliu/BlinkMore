using Avalonia;

namespace BlinkMore;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        LaunchOptions.Parse(args);
        if (LaunchOptions.Help)
        {
            Console.WriteLine("BlinkMore");
            Console.WriteLine("  --self-test                  Open the windows, switch language, and exit.");
            Console.WriteLine("  --settings <path>            Use this settings file.");
            Console.WriteLine("  --screenshot-dir <path>      Where --self-test writes screenshots.");
            return;
        }

        try
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            Log.Error(ex.ToString());
            throw;
        }
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
