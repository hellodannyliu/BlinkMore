using System.Reflection;

namespace BlinkMore.Vision;

public readonly record struct CascadePaths(string Face, string Eye, string Glasses);

public static class CascadeFiles
{
    public const string VersionFolder = "cascades-1";

    public static readonly string[] Names =
    [
        "haarcascade_frontalface_default.xml",
        "haarcascade_eye.xml",
        "haarcascade_eye_tree_eyeglasses.xml",
    ];

    public static CascadePaths Extract()
    {
        var directory = Path.Combine(Path.GetTempPath(), "BlinkMore", VersionFolder);
        Directory.CreateDirectory(directory);
        var assembly = typeof(CascadeFiles).Assembly;

        foreach (var name in Names)
        {
            var destination = Path.Combine(directory, name);
            using var resource = assembly.GetManifestResourceStream("BlinkMore.Vision.Cascades." + name)
                ?? throw new InvalidOperationException($"Missing embedded cascade '{name}'.");

            if (File.Exists(destination) && new FileInfo(destination).Length == resource.Length)
                continue;

            resource.Position = 0;
            using var file = File.Create(destination);
            resource.CopyTo(file);
        }

        return new CascadePaths(
            Path.Combine(directory, Names[0]),
            Path.Combine(directory, Names[1]),
            Path.Combine(directory, Names[2]));
    }
}
