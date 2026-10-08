using Avalonia.Controls;
using Avalonia.Platform;

namespace BlinkMore;

internal static class AppAssets
{
    private static WindowIcon? _icon;

    public static WindowIcon Icon => _icon ??= LoadIcon();

    private static WindowIcon LoadIcon()
    {
        using var stream = AssetLoader.Open(new Uri("avares://BlinkMore/Assets/icon_256.png"));
        return new WindowIcon(stream);
    }
}
