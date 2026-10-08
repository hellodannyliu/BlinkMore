<a href="https://www.buymeacoffee.com/oxremy" target="_blank"><img src="https://www.buymeacoffee.com/assets/img/custom_images/orange_img.png" alt="Buy Me A Coffee" style="height: 41px !important;width: 174px !important;box-shadow: 0px 3px 2px 0px rgba(190, 190, 190, 0.5) !important;-webkit-box-shadow: 0px 3px 2px 0px rgba(190, 190, 190, 0.5) !important;" ></a>

![BlinkMoreFree Screenshot](BlinkMoreFreeScreenCap.png "BlinkMoreFree")

**English** | [中文](README.zh-CN.md)

# BlinkMoreFree

BlinkMoreFree is an open source menu bar application that helps reduce eye strain by fading your screen when you stare at it for too long without blinking. It uses automated eye tracking and customizable settings to encourage you to blink more.

It is available for:

- **macOS** (the original app, in [`BlinkMore/`](BlinkMore/))
- **Windows** (a system tray app, in [`windows/`](windows/), with English and 中文 interfaces)

If you like the free version, consider [BlinkMore](http://oxremy.github.io/BuyBlinkMore/). The main difference: eye tracking turns on or off based on the app you are using, saving energy by only running for the apps you choose. It is well suited to text-heavy work like reading and research.

## macOS

### Requirements

- macOS 14 (Sonoma) or later
- Uses the Mac's built-in front-facing camera

### Install

Download `BlinkMoreFree.dmg` from this repository.

### Tips for the best experience

- Make sure the Mac's camera has a clear view of your eyes. Glasses at certain angles may reflect and block the camera's view.
- Consider the camera angle. Blink detection may not work at extreme angles, such as lying in bed with the Mac on your lap.
- BlinkMoreFree is well suited to reading and other text-heavy tasks. It does use a good amount of power.

## Windows

BlinkMore runs on 64-bit Windows 10 and Windows 11. It stays in the system tray.

### Install and run

1. Download [`BlinkMore-Windows-x64.zip`](BlinkMore-Windows-x64.zip) from this repository.
2. Unzip it.
3. Run `BlinkMore.exe`. No separate .NET install is needed.
4. Right-click the eye icon in the system tray (bottom-right of the taskbar) to open the menu.

The first launch asks whether to use the camera. You can skip it and turn eye tracking on later in the settings.

### Switch language

Switch between **English** and **中文** in either place. The choice is saved.

### CPU or Intel graphics

Face detection can run on the CPU or on Intel graphics built into the processor, including Iris Xe (for example an 11th-generation Core i5-1155G7). Choose **CPU only** or **Intel graphics** under Processing in the settings window. Eye open/closed detection stays on the CPU either way. If no Intel GPU is available, BlinkMore keeps using the CPU and says so.

- Tray menu: **Language**
- Settings window: the buttons at the top

### Settings

- Time between blinks: 3 to 12 seconds
- Fade duration: 1 to 5 seconds
- Blink sensitivity: low, medium, or high
- Fade color: nine colors
- Camera selection

If the screen stays faded for 6 seconds, eye tracking turns itself off so the overlay does not keep blocking you.

### Build from source

Install the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0), then:

```bash
./windows/build.sh          # Linux / macOS
```

```powershell
./windows/build.ps1         # Windows
```

Run the tests with `dotnet test windows/BlinkMore.sln -c Release`. More details are in [windows/README.md](windows/README.md).

## Privacy

Everything happens on your own computer. No data leaves your device or is kept after you close the app. No account or personal information is needed.

## Credits

Made with ❤️ by [oxremy](https://github.com/oxremy) and AI (Grok/Claude).
Windows port: see [windows/README.md](windows/README.md).
