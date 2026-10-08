namespace BlinkMore.Core;

public readonly record struct Letterbox(int X, int Y, int Width, int Height, double Scale)
{
    public static Letterbox Fit(int sourceWidth, int sourceHeight, int destination)
    {
        if (sourceWidth <= 0 || sourceHeight <= 0 || destination <= 0)
            throw new ArgumentOutOfRangeException(nameof(sourceWidth));

        var scale = Math.Min((double)destination / sourceWidth, (double)destination / sourceHeight);
        var width = Math.Clamp((int)Math.Round(sourceWidth * scale), 1, destination);
        var height = Math.Clamp((int)Math.Round(sourceHeight * scale), 1, destination);
        var x = (destination - width) / 2;
        var y = (destination - height) / 2;
        return new Letterbox(x, y, width, height, width / (double)sourceWidth);
    }

    public bool TryMapToSource(int faceX, int faceY, int faceWidth, int faceHeight, int sourceWidth, int sourceHeight, out int x, out int y, out int width, out int height)
    {
        x = y = width = height = 0;
        if (Scale <= 0 || sourceWidth <= 0 || sourceHeight <= 0)
            return false;

        var mappedX = (int)Math.Round((faceX - X) / Scale);
        var mappedY = (int)Math.Round((faceY - Y) / Scale);
        var mappedW = Math.Max(1, (int)Math.Round(faceWidth / Scale));
        var mappedH = Math.Max(1, (int)Math.Round(faceHeight / Scale));
        var left = Math.Clamp(mappedX, 0, sourceWidth - 1);
        var top = Math.Clamp(mappedY, 0, sourceHeight - 1);
        var right = Math.Clamp(mappedX + mappedW, left + 1, sourceWidth);
        var bottom = Math.Clamp(mappedY + mappedH, top + 1, sourceHeight);
        x = left;
        y = top;
        width = right - left;
        height = bottom - top;
        return width >= 20 && height >= 20;
    }
}
