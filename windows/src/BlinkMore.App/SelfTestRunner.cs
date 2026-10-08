using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using BlinkMore.Core;

namespace BlinkMore;

internal static class SelfTestRunner
{
    public static async Task RunAsync(
        AppController controller,
        TrayController tray,
        SettingsWindow settings,
        string? screenshotDir,
        IClassicDesktopStyleApplicationLifetime desktop)
    {
        var directory = screenshotDir ?? Path.Combine(Path.GetTempPath(), "blinkmore-selftest");
        Directory.CreateDirectory(directory);
        try
        {
            await Task.Delay(400);
            await OnUi(() =>
            {
                Require(settings.TitleDisplayed == TextCatalog.Get(AppLanguage.English, TextKey.SettingsTitle),
                    "English settings title was '" + settings.TitleDisplayed + "'.");
                SaveShot(settings, Path.Combine(directory, "settings-en.png"));

                controller.SetLanguage(AppLanguage.Chinese);
                Require(settings.TitleDisplayed == "设置", "Chinese title was '" + settings.TitleDisplayed + "'.");
                Require(settings.TaglineDisplayed.Contains('眨', StringComparison.Ordinal),
                    "Chinese tagline was '" + settings.TaglineDisplayed + "'.");
                Require(tray.MenuHeaders.Contains("退出"), "Tray menu was not rebuilt in Chinese.");
                SaveShot(settings, Path.Combine(directory, "settings-zh.png"));

                var how = new HowItWorksWindow(controller);
                how.Show();
                how.UpdateLayout();
                Require(how.IntroDisplayed.Contains("托盘", StringComparison.Ordinal),
                    "How-it-works text was '" + how.IntroDisplayed + "'.");
                SaveShot(how, Path.Combine(directory, "how-zh.png"));
                how.Close();

                var onboarding = new OnboardingWindow(controller, preview: true);
                onboarding.Show();
                onboarding.UpdateLayout();
                Require(onboarding.WelcomeDisplayed.Contains("欢迎", StringComparison.Ordinal),
                    "Onboarding title was '" + onboarding.WelcomeDisplayed + "'.");
                SaveShot(onboarding, Path.Combine(directory, "onboarding-zh.png"));
                onboarding.Close();

                controller.SetLanguage(AppLanguage.English);
                Require(settings.TitleDisplayed == "Settings", "English title did not return.");

                controller.SetFadeColor("#E63333");
                controller.SetBlinkThreshold(8);
                controller.SetFadeSpeed(2);
                controller.SetSensitivity(SensitivityLevel.High);
                var reloaded = SettingsStore.Load(LaunchOptions.SettingsPath);
                Require(reloaded.Language == AppLanguage.English, "Saved language was not English.");
                Require(reloaded.FadeColorHex == "#E63333", "Saved color was " + reloaded.FadeColorHex);
                Require(Math.Abs(reloaded.BlinkThresholdSeconds - 8) < 0.01, "Saved blink threshold was not 8.");
                Require(reloaded.SensitivityLevel == SensitivityLevel.High, "Saved sensitivity was not high.");

                controller.Overlay.Apply(Avalonia.Media.Colors.Red, TimeSpan.Zero, immediate: true);
                Require(controller.Overlay.IsOverlayVisible, "Fade overlay did not show.");
                Require(controller.Overlay.Opacity > 0.99, "Fade opacity was " + controller.Overlay.Opacity);
                controller.Overlay.Remove(TimeSpan.Zero);
                Require(!controller.Overlay.IsOverlayVisible, "Fade overlay stayed visible.");

                controller.Overlay.Apply(Avalonia.Media.Colors.Black, TimeSpan.FromMilliseconds(120), immediate: false);
            });

            await Task.Delay(500);
            await OnUi(() =>
            {
                Require(controller.Overlay.Opacity > 0.7, "Animated fade only reached " + controller.Overlay.Opacity);
                SaveShot(settings, Path.Combine(directory, "settings-after-fade.png"));
                controller.Overlay.Remove(TimeSpan.Zero);
            });

            await Finish(desktop, directory, 0, "ok");
        }
        catch (Exception ex)
        {
            Log.Error(ex.ToString());
            await Finish(desktop, directory, 1, ex.ToString());
        }
    }

    private static Task OnUi(Action action)
        => Dispatcher.UIThread.InvokeAsync(action).GetTask();

    private static void Require(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException(message);
    }

    private static void SaveShot(Control control, string path)
    {
        control.UpdateLayout();
        var width = Math.Max(32, (int)Math.Ceiling(control.Bounds.Width));
        var height = Math.Max(32, (int)Math.Ceiling(control.Bounds.Height));
        var bitmap = new RenderTargetBitmap(new Avalonia.PixelSize(width, height), new Avalonia.Vector(96, 96));
        bitmap.Render(control);
        bitmap.Save(path);
    }

    private static async Task Finish(
        IClassicDesktopStyleApplicationLifetime desktop,
        string directory,
        int code,
        string message)
    {
        await OnUi(() =>
        {
            File.WriteAllText(Path.Combine(directory, "result.txt"), message);
            desktop.Shutdown(code);
        });
    }
}
