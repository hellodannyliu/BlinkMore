using Avalonia.Controls;
using Avalonia.Interactivity;

namespace BlinkMore;

internal partial class OnboardingWindow : Window
{
    private readonly AppController _controller;
    private readonly OnboardingViewModel _model;
    private readonly bool _preview;
    private bool _finished;

    public OnboardingWindow(AppController controller, bool preview = false)
    {
        _controller = controller;
        _preview = preview;
        _model = new OnboardingViewModel(controller);
        DataContext = _model;
        InitializeComponent();
        Icon = AppAssets.Icon;
        _controller.DisplayChanged += OnChanged;
    }

    public string WelcomeDisplayed => WelcomeTitle.Text ?? "";

    protected override void OnClosed(EventArgs e)
    {
        _controller.DisplayChanged -= OnChanged;
        base.OnClosed(e);
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (!_finished && !_preview)
        {
            _finished = true;
            _ = _controller.CompleteOnboardingAsync(false);
        }

        base.OnClosing(e);
    }

    private void OnChanged() => _model.Refresh();

    private async void OnContinue(object? sender, RoutedEventArgs e)
    {
        if (_finished)
            return;
        _finished = true;
        if (!_preview)
            await _controller.CompleteOnboardingAsync(true);
        Close();
    }

    private async void OnSkip(object? sender, RoutedEventArgs e)
    {
        if (_finished)
            return;
        _finished = true;
        if (!_preview)
            await _controller.CompleteOnboardingAsync(false);
        Close();
    }
}
