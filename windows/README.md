# BlinkMore for Windows

**English** | [中文](README.zh-CN.md)

BlinkMore for Windows is a system tray app. It uses the camera to watch whether you blink. If you stare at the screen longer than the interval you set, the screen fades; a blink brings it back right away.

The macOS app in [`../BlinkMore/`](../BlinkMore/) is unchanged. This folder is the Windows port. It is written in C# with .NET 8 and Avalonia for the UI, and uses OpenCV for face and eye detection.

## Run the app

Requirements: 64-bit Windows 10 or Windows 11. No separate .NET install is needed.

1. Download [`../BlinkMore-Windows-x64.zip`](../BlinkMore-Windows-x64.zip) from the repository root.
2. Unzip it.
3. Run `BlinkMore.exe`.
4. Right-click the tray icon in the bottom-right of the taskbar.

The first launch asks whether to use the camera. You can skip it and enable eye tracking later.

### Switch language

Language can be switched in two places, with the same result:

- Tray menu: **Language** (语言)
- Settings window: the **English** / **中文** buttons at the top

The choice is saved in `%APPDATA%\BlinkMore\settings.json`.

## Build from source

Install the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or newer.

```bash
./windows/build.sh
```

On Windows:

```powershell
./windows/build.ps1
```

The output is a single self-contained `dist/BlinkMore-Windows/BlinkMore.exe`, which is also packaged as `BlinkMore-Windows-x64.zip` in the repository root. The zip also contains a short user guide, the app license, and the font license (`NotoSansSC-OFL.txt`). Other third-party notices are in [`THIRD-PARTY-NOTICES.md`](THIRD-PARTY-NOTICES.md).

Run the unit tests:

```bash
dotnet test windows/BlinkMore.sln -c Release
```

Run the desktop self-test. It needs a display; on Linux it uses Xvfb:

```bash
./windows/self-test.sh
```

The self-test opens the windows, switches language, saves settings, and checks the fade overlay. Screenshots are written to `self-test-output/` by default.

## Features

Same as the macOS version:

- Turn eye tracking on or off
- Time between blinks: 3 to 12 seconds
- Fade duration: 1 to 5 seconds
- Blink sensitivity: low, medium, or high
- Nine fade colors
- Camera selection
- Tracking turns itself off after 6 seconds of continuous fade

Video is processed on this computer only. It is not uploaded or saved.

## Layout

| Path | Contents |
| --- | --- |
| `src/BlinkMore.Core` | Settings, language catalog, blink and fade rules (no UI, unit tested) |
| `src/BlinkMore.Vision` | Camera capture and OpenCV face/eye analysis |
| `src/BlinkMore.App` | Avalonia tray app, settings, onboarding, fade overlay |
| `tests/` | xUnit tests |
| `tools/subset-font.py` | Rebuilds the embedded Noto Sans SC subset |
| `third-party/` and `THIRD-PARTY-NOTICES.md` | Licenses for bundled assets |

## Notes

- Chinese and English text share one font subset, `Noto Sans SC`, embedded in the app so it renders the same on any Windows install. After you change UI text, run `python3 tools/subset-font.py` with the full Noto Sans SC files available (see the script for the download location).
- Logs go to `%APPDATA%\BlinkMore\blinkmore.log`.
