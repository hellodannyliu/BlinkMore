using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace BlinkMore;

internal partial class SettingsWindow : Window
{
    private readonly AppController _controller;
    private readonly SettingsViewModel _model;

    public SettingsWindow(AppController controller)
    {
        _controller = controller;
        _model = new SettingsViewModel(_controller);
        DataContext = _model;
        InitializeComponent();
        Icon = AppAssets.Icon;
        _model.Rebuild();
        _controller.DisplayChanged += OnChanged;
        _controller.TrackingChanged += OnChanged;
        _controller.CamerasChanged += OnChanged;
        Opened += (_, _) => _controller.RefreshCameras();
    }

    public string TitleDisplayed => TitleText.Text ?? "";
    public string TaglineDisplayed => TaglineText.Text ?? "";

    protected override void OnClosed(EventArgs e)
    {
        _controller.DisplayChanged -= OnChanged;
        _controller.TrackingChanged -= OnChanged;
        _controller.CamerasChanged -= OnChanged;
        base.OnClosed(e);
    }

    private void OnChanged()
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(OnChanged);
            return;
        }

        _model.Rebuild();
    }

    private void OnEnglish(object? sender, RoutedEventArgs e) => _controller.SetLanguage(Core.AppLanguage.English);

    private void OnChinese(object? sender, RoutedEventArgs e) => _controller.SetLanguage(Core.AppLanguage.Chinese);

    private void OnPreview(object? sender, RoutedEventArgs e) => _controller.PreviewFade();

    private void OnHowItWorks(object? sender, RoutedEventArgs e) => _controller.ShowHowItWorks?.Invoke();

    private void OnMadeBy(object? sender, RoutedEventArgs e) => SystemSettings.OpenAuthorPage();

    private void OnColorClick(object? sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.Tag is string hex)
            _controller.SetFadeColor(hex);
    }

    private void OnCameraChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is ComboBox box && box.SelectedItem is CameraOption option)
            _controller.SetCamera(option.Id);
    }
}
