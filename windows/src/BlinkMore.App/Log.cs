namespace BlinkMore;

internal static class Log
{
    private static readonly object Gate = new();

    public static void Info(string message) => Write("INFO", message);

    public static void Error(string message) => Write("ERROR", message);

    private static void Write(string level, string message)
    {
        var line = $"{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss} {level} {message}";
        Console.Error.WriteLine(line);
        try
        {
            var path = Path.Combine(
                Path.GetDirectoryName(BlinkMore.Core.SettingsStore.DefaultPath())!,
                "blinkmore.log");
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);
            lock (Gate)
                File.AppendAllText(path, line + Environment.NewLine);
        }
        catch (IOException)
        {
            // Logging must not take the app down.
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}
