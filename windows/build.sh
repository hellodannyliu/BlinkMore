#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")" && pwd)"
REPO="$(cd "$ROOT/.." && pwd)"
OUT="$REPO/dist/BlinkMore-Windows"

export DOTNET_CLI_TELEMETRY_OPTOUT=1

dotnet test "$ROOT/BlinkMore.sln" -c Release
dotnet publish "$ROOT/src/BlinkMore.App/BlinkMore.App.csproj" \
  -c Release \
  -r win-x64 \
  --self-contained true \
  -p:PublishSingleFile=true \
  -p:IncludeNativeLibrariesForSelfExtract=true \
  -p:EnableCompressionInSingleFile=true \
  -p:DebugType=none \
  -p:DebugSymbols=false \
  -o "$OUT"

cat > "$OUT/使用说明.txt" << 'EOF'
BlinkMore for Windows
=====================

运行 BlinkMore.exe。程序会出现在系统托盘（任务栏右下角）。
第一次打开会询问是否使用摄像头。可以先跳过，之后在设置里再打开。

语言 / Language
在托盘菜单的「语言」里选择 English 或 中文，也可以在设置窗口顶部切换。
选择会被记住。

设置
- 两次眨眼间隔：3 到 12 秒
- 淡出时长：1 到 5 秒
- 眨眼灵敏度：低 / 中 / 高
- 淡出颜色
- 摄像头

如果画面变暗，眨一下眼就会恢复。持续变暗 6 秒后，检测会自动关闭。

隐私：画面只在这台电脑上处理，不会上传，也不会保存。

---

Run BlinkMore.exe. The app lives in the system tray.
The first launch asks to use the camera. You can skip it and turn tracking on later.

Switch language from the tray menu (语言 / Language) or the top of the settings window.
The choice is saved.

If the screen fades, blink and it comes back. Tracking turns itself off after 6 seconds of fade.
Video stays on this computer and is not uploaded or saved.
EOF

cp "$REPO/LICENSE.md" "$OUT/LICENSE.md"
cp "$ROOT/third-party/NotoSansSC/OFL.txt" "$OUT/NotoSansSC-OFL.txt"

(
  cd "$REPO/dist"
  rm -f BlinkMore-Windows-x64.zip
  zip -r -q BlinkMore-Windows-x64.zip BlinkMore-Windows
)

cp "$REPO/dist/BlinkMore-Windows-x64.zip" "$REPO/BlinkMore-Windows-x64.zip"
echo "Published $OUT/BlinkMore.exe"
echo "Zip: $REPO/BlinkMore-Windows-x64.zip"
