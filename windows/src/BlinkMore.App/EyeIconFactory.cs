using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace BlinkMore;

internal static class EyeIconFactory
{
    public static WindowIcon Create(bool open)
    {
        var root = new Canvas
        {
            Width = 32,
            Height = 32,
            Background = Brushes.Transparent,
        };
        root.Children.Add(new Avalonia.Controls.Shapes.Ellipse
        {
            Width = 32,
            Height = 32,
            Fill = new SolidColorBrush(Color.Parse("#1F4B3A")),
        });

        if (open)
        {
            var outline = new Avalonia.Controls.Shapes.Ellipse
            {
                Width = 18,
                Height = 11,
                Stroke = Brushes.White,
                StrokeThickness = 1.6,
                Fill = Brushes.Transparent,
            };
            Canvas.SetLeft(outline, 7);
            Canvas.SetTop(outline, 10.5);
            root.Children.Add(outline);

            var pupil = new Avalonia.Controls.Shapes.Ellipse
            {
                Width = 4.5,
                Height = 4.5,
                Fill = Brushes.White,
            };
            Canvas.SetLeft(pupil, 13.75);
            Canvas.SetTop(pupil, 13.75);
            root.Children.Add(pupil);
        }
        else
        {
            root.Children.Add(new Avalonia.Controls.Shapes.Path
            {
                Data = Geometry.Parse("M7,17 C11,13 21,13 25,17"),
                Stroke = Brushes.White,
                StrokeThickness = 1.8,
                StrokeJoin = PenLineJoin.Round,
                StrokeLineCap = PenLineCap.Round,
            });
        }

        root.Measure(new Size(32, 32));
        root.Arrange(new Rect(0, 0, 32, 32));
        var bitmap = new RenderTargetBitmap(new PixelSize(64, 64), new Vector(192, 192));
        bitmap.Render(root);
        var stream = new MemoryStream();
        bitmap.Save(stream);
        stream.Position = 0;
        return new WindowIcon(stream);
    }
}
