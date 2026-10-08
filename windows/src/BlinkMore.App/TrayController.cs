using Avalonia;
using Avalonia.Controls;
using BlinkMore.Core;

namespace BlinkMore;

internal sealed class TrayController
{
    private readonly AppController _controller;
    private readonly WindowIcon _openIcon;
    private readonly WindowIcon _closedIcon;
    private TrayIcon? _tray;

    public TrayController(AppController controller)
    {
        _controller = controller;
        _openIcon = EyeIconFactory.Create(open: true);
        _closedIcon = EyeIconFactory.Create(open: false);
    }

    public IReadOnlyList<string> MenuHeaders { get; private set; } = [];

    public void Attach()
    {
        _tray = new TrayIcon
        {
            Icon = _closedIcon,
            ToolTipText = _controller.Loc[TextKey.TrayTooltip],
            Menu = BuildMenu(),
        };
        var icons = new TrayIcons { _tray };
        TrayIcon.SetIcons(Application.Current!, icons);
        _controller.DisplayChanged += Rebuild;
        _controller.TrackingChanged += Rebuild;
        _controller.EyeChanged += UpdateIcon;
    }

    private void Rebuild()
    {
        if (_tray == null)
            return;
        _tray.ToolTipText = _controller.Loc[TextKey.TrayTooltip];
        _tray.Menu = BuildMenu();
        UpdateIcon();
    }

    private void UpdateIcon()
    {
        if (_tray == null)
            return;
        var open = _controller.Settings.EyeTrackingEnabled && _controller.EyesOpen;
        _tray.Icon = open ? _openIcon : _closedIcon;
    }

    private NativeMenu BuildMenu()
    {
        var loc = _controller.Loc;
        var settings = _controller.Settings;
        var menu = new NativeMenu();

        var tracking = new NativeMenuItem(loc[TextKey.EnableTracking])
        {
            ToggleType = NativeMenuItemToggleType.CheckBox,
            IsChecked = settings.EyeTrackingEnabled,
        };
        tracking.Click += (_, _) => _ = _controller.SetEyeTrackingAsync(tracking.IsChecked);
        menu.Add(tracking);

        var openSettings = new NativeMenuItem(loc[TextKey.OpenSettings]);
        openSettings.Click += (_, _) => _controller.ShowSettings?.Invoke();
        menu.Add(openSettings);
        menu.Add(new NativeMenuItemSeparator());

        var language = new NativeMenuItem(loc[TextKey.Language]);
        var languageMenu = new NativeMenu();
        languageMenu.Add(LanguageItem(loc[TextKey.LangEn], AppLanguage.English));
        languageMenu.Add(LanguageItem(loc[TextKey.LangZh], AppLanguage.Chinese));
        language.Menu = languageMenu;
        menu.Add(language);

        var how = new NativeMenuItem(loc[TextKey.HowItWorks]);
        how.Click += (_, _) => _controller.ShowHowItWorks?.Invoke();
        menu.Add(how);
        menu.Add(new NativeMenuItemSeparator());

        var credit = new NativeMenuItem(loc[TextKey.MadeBy]);
        credit.Click += (_, _) => SystemSettings.OpenAuthorPage();
        menu.Add(credit);

        var quit = new NativeMenuItem(loc[TextKey.Quit]);
        quit.Click += (_, _) => _controller.Quit();
        menu.Add(quit);

        MenuHeaders = menu.Items
            .OfType<NativeMenuItem>()
            .Select(item => item.Header?.ToString() ?? "")
            .ToArray();
        return menu;
    }

    private NativeMenuItem LanguageItem(string header, AppLanguage language)
    {
        var item = new NativeMenuItem(header)
        {
            ToggleType = NativeMenuItemToggleType.Radio,
            IsChecked = _controller.Settings.Language == language,
        };
        item.Click += (_, _) => _controller.SetLanguage(language);
        return item;
    }
}
