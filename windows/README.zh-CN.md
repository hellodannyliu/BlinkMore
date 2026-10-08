# BlinkMore Windows 版

[English](README.md) | **中文**

BlinkMore Windows 版是一个系统托盘程序。它用摄像头观察你有没有眨眼。如果盯着屏幕的时间超过你设置的间隔，画面会变暗；眨一下眼，画面立刻恢复。

macOS 版位于 [`../BlinkMore/`](../BlinkMore/)，没有改动。本目录是 Windows 移植版，使用 C# 和 .NET 8 编写，界面使用 Avalonia，人脸和眼睛检测使用 OpenCV。

## 直接运行

运行要求：64 位 Windows 10 或 Windows 11。不需要另外安装 .NET。

1. 下载仓库根目录的 [`../BlinkMore-Windows-x64.zip`](../BlinkMore-Windows-x64.zip)。
2. 解压。
3. 运行 `BlinkMore.exe`。
4. 在任务栏右下角的托盘图标上点右键。

第一次打开时会询问是否使用摄像头。可以先跳过，之后再开启眨眼检测。

### 切换语言

以下两个位置都可以切换语言，效果相同：

- 托盘菜单中的 **语言**（Language）
- 设置窗口顶部的 **English** / **中文** 按钮

选择会保存在 `%APPDATA%\BlinkMore\settings.json` 中。

### 用 CPU 还是 Intel 核显

在 **处理方式** 中选择 **仅 CPU** 或 **Intel 核显**。Intel 核显通过 OpenCL 使用处理器里的集成显卡；如果有 Iris Xe，会优先用它（11 代酷睿，例如 i5-1155G7，自带 Intel Iris Xe）。选中之后，人脸检测在这块核显上运行。眼睛是否睁开仍由 CPU 判断，而且只处理脸部那一小块画面。如果 Intel 核显无法启动，程序会说明原因，并继续在 CPU 上检测人脸。

## 从源码编译

先安装 [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) 或更新的版本。

```bash
./windows/build.sh
```

在 Windows 上：

```powershell
./windows/build.ps1
```

输出是一个自包含的单文件 `dist/BlinkMore-Windows/BlinkMore.exe`，同时打包为仓库根目录的 `BlinkMore-Windows-x64.zip`。压缩包里还有使用说明、应用许可证和字体许可证（`NotoSansSC-OFL.txt`）。其他第三方声明见 [`THIRD-PARTY-NOTICES.md`](THIRD-PARTY-NOTICES.md)。

运行单元测试：

```bash
dotnet test windows/BlinkMore.sln -c Release
```

运行界面自测。它需要图形环境；在 Linux 上使用 Xvfb：

```bash
./windows/self-test.sh
```

自测会打开各个窗口、切换语言、保存设置，并检查淡出效果。截图默认保存在 `self-test-output/` 中。

## 功能

与 macOS 版相同：

- 开启或关闭眨眼检测
- 两次眨眼间隔：3 到 12 秒
- 淡出时长：1 到 5 秒
- 眨眼灵敏度：低、中、高
- 九种淡出颜色
- 摄像头选择
- 画面持续变暗 6 秒后，眨眼检测会自动关闭

画面只在这台电脑上处理，不会上传，也不会保存。

## 目录结构

| 路径 | 内容 |
| --- | --- |
| `src/BlinkMore.Core` | 设置、语言文案、眨眼与淡出规则（不含界面，有单元测试） |
| `src/BlinkMore.Vision` | 摄像头采集与 OpenCV 人脸、眼睛分析 |
| `src/BlinkMore.App` | Avalonia 托盘程序、设置窗口、首次引导、淡出遮罩 |
| `tests/` | xUnit 测试 |
| `tools/subset-font.py` | 重新生成内嵌的 Noto Sans SC 字体子集 |
| `third-party/` 和 `THIRD-PARTY-NOTICES.md` | 内置资源的许可证 |

## 注意事项

- 中英文共用内嵌的 `Noto Sans SC` 字体子集，这样在任何 Windows 上显示都一致。修改界面文字后，需要准备完整的 Noto Sans SC 字体文件，再运行 `python3 tools/subset-font.py`（下载地址见脚本说明）。
- 日志保存在 `%APPDATA%\BlinkMore\blinkmore.log`。
