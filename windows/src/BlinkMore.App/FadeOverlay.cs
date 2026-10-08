using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Threading;

namespace BlinkMore;

internal sealed class FadeOverlay : IDisposable
{
    private readonly List<FadeWindow> _windows = [];
    private readonly DispatcherTimer _timer;
    private Color _color = Colors.Black;
    private double _opacity;
    private double _target;
    private double _perSecond = 1;
    private bool _visible;
    private DateTime _lastTick;

    public FadeOverlay()
    {
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        _timer.Tick += (_, _) => OnTick();
    }

    public double Opacity => _opacity;
    public bool IsOverlayVisible => _visible;

    public void Apply(Color color, TimeSpan duration, bool immediate)
    {
        _color = color;
        EnsureWindows();
        foreach (var window in _windows)
            window.SetColor(color);

        _target = 1;
        if (immediate || duration <= TimeSpan.Zero)
        {
            _opacity = 1;
            ShowAll();
            PushOpacity();
            _timer.Stop();
            return;
        }

        _perSecond = 1.0 / duration.TotalSeconds;
        _lastTick = DateTime.UtcNow;
        ShowAll();
        _timer.Start();
    }

    public void Remove(TimeSpan duration)
    {
        _target = 0;
        if (duration <= TimeSpan.Zero)
        {
            _opacity = 0;
            PushOpacity();
            HideAll();
            _timer.Stop();
            return;
        }

        _perSecond = 1.0 / duration.TotalSeconds;
        _lastTick = DateTime.UtcNow;
        if (_visible)
            _timer.Start();
    }

    public void Dispose()
    {
        _timer.Stop();
        foreach (var window in _windows)
            window.Close();
        _windows.Clear();
        _visible = false;
    }

    private void OnTick()
    {
        var now = DateTime.UtcNow;
        var elapsed = Math.Clamp((now - _lastTick).TotalSeconds, 0, 0.1);
        _lastTick = now;
        var step = _perSecond * elapsed;
        if (_opacity < _target)
            _opacity = Math.Min(_target, _opacity + step);
        else if (_opacity > _target)
            _opacity = Math.Max(_target, _opacity - step);

        PushOpacity();
        if (Math.Abs(_opacity - _target) > 0.01)
            return;

        _opacity = _target;
        PushOpacity();
        _timer.Stop();
        if (_opacity <= 0)
            HideAll();
    }

    private void ShowAll()
    {
        foreach (var window in _windows)
        {
            if (!window.IsVisible)
                window.Show();
            window.Topmost = true;
            WindowClickThrough.Apply(window);
        }

        _visible = _windows.Count > 0;
    }

    private void HideAll()
    {
        foreach (var window in _windows)
        {
            if (window.IsVisible)
                window.Hide();
        }

        _visible = false;
    }

    private void PushOpacity()
    {
        foreach (var window in _windows)
            window.SetOpacity(_opacity);
    }

    private void EnsureWindows()
    {
        var screens = ReadScreens();
        if (screens.Count == 0)
        {
            if (_windows.Count == 0)
                _windows.Add(new FadeWindow());
            return;
        }

        if (_windows.Count == screens.Count)
        {
            for (var i = 0; i < screens.Count; i++)
                _windows[i].Fit(screens[i]);
            return;
        }

        foreach (var window in _windows)
            window.Close();
        _windows.Clear();
        foreach (var screen in screens)
        {
            var window = new FadeWindow();
            window.Fit(screen);
            window.SetColor(_color);
            window.SetOpacity(_opacity);
            _windows.Add(window);
        }
    }

    private IReadOnlyList<Screen> ReadScreens()
    {
        var existing = _windows.FirstOrDefault();
        var known = existing?.Screens?.All?.ToList() ?? [];
        if (known.Count > 0)
            return known;

        var probe = existing ?? new FadeWindow();
        var addedProbe = existing == null;
        if (!probe.IsVisible)
            probe.Show();
        var found = probe.Screens?.All?.ToList() ?? [];
        if (found.Count == 0)
        {
            if (addedProbe)
                _windows.Add(probe);
            return found;
        }

        if (addedProbe)
            probe.Close();
        return found;
    }
}

internal sealed class FadeWindow : Window
{
    private readonly Border _layer;

    public FadeWindow()
    {
        SystemDecorations = SystemDecorations.None;
        TransparencyLevelHint = [WindowTransparencyLevel.Transparent];
        Background = Brushes.Transparent;
        Topmost = true;
        ShowInTaskbar = false;
        ShowActivated = false;
        CanResize = false;
        Focusable = false;
        Width = 800;
        Height = 600;
        _layer = new Border
        {
            Background = Brushes.Black,
            Opacity = 0,
            IsHitTestVisible = false,
        };
        Content = _layer;
    }

    public void SetColor(Color color) => _layer.Background = new SolidColorBrush(color);

    public void SetOpacity(double opacity) => _layer.Opacity = opacity;

    public void Fit(Screen screen)
    {
        WindowState = WindowState.Normal;
        Position = screen.Bounds.Position;
        Width = Math.Max(1, screen.Bounds.Width / screen.Scaling);
        Height = Math.Max(1, screen.Bounds.Height / screen.Scaling);
    }
}
