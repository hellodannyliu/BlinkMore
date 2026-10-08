using Avalonia.Controls;
using Avalonia.Interactivity;

namespace BlinkMore;

internal partial class HowItWorksWindow : Window
{
    private readonly AppController _controller;
    private readonly HowItWorksViewModel _model;

    public HowItWorksWindow(AppController controller)
    {
        _controller = controller;
        _model = new HowItWorksViewModel(controller);
        DataContext = _model;
        InitializeComponent();
        Icon = AppAssets.Icon;
        _controller.DisplayChanged += OnChanged;
    }

    public string IntroDisplayed => IntroText.Text ?? "";

    protected override void OnClosed(EventArgs e)
    {
        _controller.DisplayChanged -= OnChanged;
        base.OnClosed(e);
    }

    private void OnChanged() => _model.Refresh();

    private void OnClose(object? sender, RoutedEventArgs e) => Close();
}
