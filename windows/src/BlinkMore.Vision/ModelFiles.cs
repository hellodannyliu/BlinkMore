using System.Reflection;

namespace BlinkMore.Vision;

internal static class ModelFiles
{
    public const string YunetName = "face_detection_yunet_2023mar.onnx";

    public static string EnsureYunet()
    {
        var directory = Path.Combine(Path.GetTempPath(), "BlinkMore", "models-1");
        Directory.CreateDirectory(directory);
        var destination = Path.Combine(directory, YunetName);
        using var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream("BlinkMore.Vision.Models." + YunetName)
            ?? throw new InvalidOperationException("The YuNet face model is missing from the app.");

        if (!File.Exists(destination) || new FileInfo(destination).Length != resource.Length)
        {
            resource.Position = 0;
            using var file = File.Create(destination);
            resource.CopyTo(file);
        }

        return destination;
    }
}
