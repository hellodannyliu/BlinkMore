using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using BlinkMore.Core;

namespace BlinkMore;

public partial class App : Application
{
    private Mutex? _mutex;
    private bool _ownsMutex;
    private AppShell? _shell;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            if (!OwnInstance(desktop))
            {
                base.OnFrameworkInitializationCompleted();
                return;
            }

            _shell = new AppShell(desktop);
            _shell.Start();
            desktop.Exit += (_, _) =>
            {
                _shell.Dispose();
                ReleaseMutex();
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private bool OwnInstance(IClassicDesktopStyleApplicationLifetime desktop)
    {
        if (LaunchOptions.SelfTest)
            return true;

        try
        {
            _mutex = new Mutex(true, "BlinkMoreFree", out var created);
            _ownsMutex = created;
            if (created)
                return true;
        }
        catch (AbandonedMutexException)
        {
            _ownsMutex = true;
            return true;
        }

        var settings = SettingsStore.Load(LaunchOptions.SettingsPath);
        var loc = new LocalizationService(settings.Language);
        var window = new Window
        {
            Title = "BlinkMore",
            Width = 420,
            Height = 180,
            CanResize = false,
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
            Icon = AppAssets.Icon,
            Content = new StackPanel
            {
                Margin = new Thickness(24),
                Spacing = 16,
                Children =
                {
                    new TextBlock
                    {
                        Text = loc[TextKey.AlreadyRunning],
                        TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                        FontSize = 16,
                    },
                    new Button
                    {
                        Content = loc[TextKey.Close],
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                        Classes = { "primary" },
                    },
                },
            },
        };
        if (window.Content is StackPanel panel && panel.Children[1] is Button close)
            close.Click += (_, _) => window.Close();
        window.Closed += (_, _) => desktop.Shutdown(0);
        window.Show();
        return false;
    }

    private void ReleaseMutex()
    {
        if (!_ownsMutex || _mutex == null)
            return;
        try
        {
            _mutex.ReleaseMutex();
        }
        catch (ApplicationException)
        {
        }

        _mutex.Dispose();
        _mutex = null;
        _ownsMutex = false;
    }
}

internal sealed class AppShell : IDisposable
{
    private readonly IClassicDesktopStyleApplicationLifetime _desktop;
    private SettingsWindow? _settings;
    private bool _started;

    public AppShell(IClassicDesktopStyleApplicationLifetime desktop)
    {
        _desktop = desktop;
        var settings = SettingsStore.Load(LaunchOptions.SettingsPath);
        var localization = new LocalizationService(settings.Language);
        Controller = new AppController(settings, localization, LaunchOptions.SettingsPath);
        Tray = new TrayController(Controller);
        Controller.ShowSettings = () => ShowSettings();
        Controller.ShowHowItWorks = ShowHowItWorks;
    }

    public AppController Controller { get; }
    public TrayController Tray { get; }

    public void Start()
    {
        if (_started)
            return;
        _started = true;
        Tray.Attach();

        if (LaunchOptions.SelfTest)
        {
            var window = ShowSettings();
            _ = SelfTestRunner.RunAsync(Controller, Tray, window, LaunchOptions.ScreenshotDir, _desktop);
            return;
        }

        if (!Controller.Settings.HasShownOnboarding)
        {
            var onboarding = new OnboardingWindow(Controller);
            Controller.DialogOwner = onboarding;
            onboarding.Closed += (_, _) =>
            {
                if (Controller.DialogOwner == onboarding)
                    Controller.DialogOwner = null;
            };
            onboarding.Show();
            return;
        }

        if (Controller.Settings.EyeTrackingEnabled)
            _ = Controller.SetEyeTrackingAsync(true);
    }

    public SettingsWindow ShowSettings()
    {
        if (_settings is { IsVisible: true })
        {
            _settings.Activate();
            return _settings;
        }

        _settings = new SettingsWindow(Controller);
        Controller.DialogOwner = _settings;
        _settings.Closed += (_, _) =>
        {
            if (Controller.DialogOwner == _settings)
                Controller.DialogOwner = null;
        };
        _settings.Show();
        return _settings;
    }

    public void ShowHowItWorks()
    {
        var window = new HowItWorksWindow(Controller);
        window.Show();
    }

    public void Dispose()
    {
        Controller.Dispose();
    }
}
