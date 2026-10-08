using Avalonia.Controls;
using Avalonia.Layout;

namespace BlinkMore;

internal static class ConfirmDialog
{
    public static async Task<bool> ShowAsync(Window? owner, string title, string message, string accept, string cancel)
    {
        var accepted = false;
        var window = new Window
        {
            Title = title,
            Width = 440,
            SizeToContent = SizeToContent.Height,
            CanResize = false,
            WindowStartupLocation = owner == null
                ? WindowStartupLocation.CenterScreen
                : WindowStartupLocation.CenterOwner,
            Icon = AppAssets.Icon,
        };

        var acceptButton = new Button { Content = accept, Classes = { "primary" }, MinWidth = 120 };
        var cancelButton = new Button { Content = cancel, MinWidth = 120 };
        acceptButton.Click += (_, _) =>
        {
            accepted = true;
            window.Close();
        };
        cancelButton.Click += (_, _) => window.Close();

        window.Content = new StackPanel
        {
            Margin = new Avalonia.Thickness(22),
            Spacing = 16,
            Children =
            {
                new TextBlock { Text = message, TextWrapping = Avalonia.Media.TextWrapping.Wrap },
                new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 8,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Children = { cancelButton, acceptButton },
                },
            },
        };

        if (owner != null)
            await window.ShowDialog(owner);
        else
        {
            var closed = new TaskCompletionSource();
            window.Closed += (_, _) => closed.TrySetResult();
            window.Show();
            await closed.Task;
        }

        return accepted;
    }
}
