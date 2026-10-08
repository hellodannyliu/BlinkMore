<a href="https://www.buymeacoffee.com/oxremy" target="_blank"><img src="https://www.buymeacoffee.com/assets/img/custom_images/orange_img.png" alt="Buy Me A Coffee" style="height: 41px !important;width: 174px !important;box-shadow: 0px 3px 2px 0px rgba(190, 190, 190, 0.5) !important;-webkit-box-shadow: 0px 3px 2px 0px rgba(190, 190, 190, 0.5) !important;" ></a>

![BlinkMoreFree 截图](BlinkMoreFreeScreenCap.png "BlinkMoreFree")

[English](README.md) | **中文**

# BlinkMoreFree

BlinkMoreFree 是一款开源的菜单栏/托盘程序。盯着屏幕太久不眨眼时，它会让屏幕轻轻变暗，提醒你眨眼，帮助减轻眼睛疲劳。它通过自动眼部检测和可调节的设置，鼓励你多眨眼。

支持以下平台：

- **macOS**（原版应用，位于 [`BlinkMore/`](BlinkMore/)）
- **Windows**（系统托盘程序，位于 [`windows/`](windows/)，提供中文和英文界面）

如果你喜欢免费版，也可以考虑付费版 [BlinkMore](http://oxremy.github.io/BuyBlinkMore/)。主要区别是：付费版会根据你正在使用的应用决定是否开启眼部检测，只在你选定的应用中运行，更省电。它很适合阅读、查资料这类文字较多的工作。

## macOS

### 运行要求

- macOS 14 (Sonoma) 或更高版本
- 使用 Mac 内置的前置摄像头

### 安装

下载本仓库中的 `BlinkMoreFree.dmg`。

### 使用建议

- 确保 Mac 的摄像头能清楚看到你的眼睛。某些角度下，眼镜的反光可能会挡住摄像头的视线。
- 注意摄像头的角度。角度太偏时（比如躺在床上、把 Mac 放在腿上）眨眼检测可能无法工作。
- BlinkMoreFree 很适合阅读等文字较多的工作，但它会消耗较多电量。

## Windows

BlinkMore 可以在 64 位 Windows 10 和 Windows 11 上运行，程序会留在系统托盘里。

### 安装与运行

1. 下载本仓库中的 [`BlinkMore-Windows-x64.zip`](BlinkMore-Windows-x64.zip)。
2. 解压。
3. 运行 `BlinkMore.exe`，不需要另外安装 .NET。
4. 在任务栏右下角的托盘图标上点右键，打开菜单。

第一次打开时会询问是否使用摄像头。可以先跳过，之后在设置中再开启眨眼检测。

### 切换语言

在以下任一位置，可以在 **English** 和 **中文** 之间切换，选择会被保存：

- 托盘菜单中的 **语言**
- 设置窗口顶部的按钮

### 设置项

- 两次眨眼间隔：3 到 12 秒
- 淡出时长：1 到 5 秒
- 眨眼灵敏度：低、中、高
- 淡出颜色：九种
- 摄像头选择

如果画面持续变暗达到 6 秒，眨眼检测会自动关闭，避免画面一直挡住你。

### 从源码编译

先安装 [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)，然后：

```bash
./windows/build.sh          # Linux / macOS
```

```powershell
./windows/build.ps1         # Windows
```

运行测试：`dotnet test windows/BlinkMore.sln -c Release`。更多细节见 [windows/README.zh-CN.md](windows/README.zh-CN.md)。

## 隐私

所有处理都在你自己的电脑上完成。数据不会离开你的设备，关闭应用后也不会留存。不需要账号，也不需要提供任何个人信息。

## 致谢

由 [oxremy](https://github.com/oxremy) 用 ❤️ 制作，并借助了 AI（Grok/Claude）。
Windows 版说明：见 [windows/README.zh-CN.md](windows/README.zh-CN.md)。
