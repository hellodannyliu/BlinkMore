<a href="https://www.buymeacoffee.com/oxremy" target="_blank"><img src="https://www.buymeacoffee.com/assets/img/custom_images/orange_img.png" alt="Buy Me A Coffee" style="height: 41px !important;width: 174px !important;box-shadow: 0px 3px 2px 0px rgba(190, 190, 190, 0.5) !important;-webkit-box-shadow: 0px 3px 2px 0px rgba(190, 190, 190, 0.5) !important;" ></a>

![BlinkMoreFree Screenshot](BlinkMoreFreeScreenCap.png "BlinkMoreFree")

# BlinkMoreFree

BlinkMoreFree is an open source macOS menu bar application that helps reduce eye strain by fading your screen when you stare at it for too long without blinking. Using automated eye-tracking and customized settings to encourage you to blink more.

# BlinkMore
If you like the free version consider [BlinkMore](http://oxremy.github.io/BuyBlinkMore/). Main difference: eye tracking turns on/off based on the app you are using, saving energy by only turning on for apps you choose. I like to use with text-heavy tasks like reading/research. 

## Requirements

- macOS 14 (Sonoma) or later 
- Uses Mac's built-in front facing camera 

## Install
Download the BlinkMoreFree.dmg file above.

## Windows

BlinkMore also runs on 64-bit Windows 10 and Windows 11. Download [BlinkMore-Windows-x64.zip](BlinkMore-Windows-x64.zip), unzip it, and start `BlinkMore.exe`. The app stays in the system tray.

Switch between English and 中文 from the tray menu (**语言 / Language**) or the buttons at the top of the settings window. The choice is saved.

Build it yourself with the .NET 8 SDK:

```bash
./windows/build.sh
```

On Windows, run `windows/build.ps1`. Details are in [windows/README.md](windows/README.md).

### Windows 版

64 位 Windows 10 或 Windows 11 可以直接用。下载仓库里的 [BlinkMore-Windows-x64.zip](BlinkMore-Windows-x64.zip)，解压后运行 `BlinkMore.exe`。程序会留在系统托盘里。

托盘菜单的「语言」，或设置窗口顶部的 **English / 中文**，可以随时切换，选择会被记住。 

## Privacy

Everything happens right on your Mac—no data leaves your device or sticks around after you close the app. No accounts or personal information needed.

## Tips for Best Experience 

- Make sure your Mac's camera has a clear view of your eyes (heads-up: glasses at certain angles may have reflections that obstruct camera view).
- Consider the angle of your camera. Blink detection may not work at extreme angles, like laying in bed with Mac on your lap.
- BlinkMoreFree is perfect for text-heavy tasks like reading––just know it uses a good chunk of your Mac's power.

## Credits

Made with ❤️ by [oxremy](https://github.com/oxremy) and AI (Grok/Claude)
