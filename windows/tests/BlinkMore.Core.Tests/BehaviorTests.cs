using System.Globalization;
using Xunit;

namespace BlinkMore.Core.Tests;

public class LanguageTests
{
    [Fact]
    public void BothLanguagesDefineEveryKey()
    {
        Assert.Equal(TextKey.All.Count, TextKey.All.Distinct().Count());
        foreach (var key in TextKey.All)
        {
            var english = TextCatalog.Get(AppLanguage.English, key);
            var chinese = TextCatalog.Get(AppLanguage.Chinese, key);
            Assert.False(string.IsNullOrWhiteSpace(english), key);
            Assert.False(string.IsNullOrWhiteSpace(chinese), key);
        }
    }

    [Fact]
    public void LanguageNamesStayInTheirOwnLanguage()
    {
        Assert.Equal("English", TextCatalog.Get(AppLanguage.Chinese, TextKey.LangEn));
        Assert.Equal("中文", TextCatalog.Get(AppLanguage.English, TextKey.LangZh));
    }

    [Theory]
    [InlineData(TextKey.SettingsTitle)]
    [InlineData(TextKey.Quit)]
    [InlineData(TextKey.HowIntro)]
    [InlineData(TextKey.WelcomeBody)]
    [InlineData(TextKey.EnableTracking)]
    public void ImportantCopyIsActuallyTranslated(string key)
    {
        Assert.NotEqual(
            TextCatalog.Get(AppLanguage.English, key),
            TextCatalog.Get(AppLanguage.Chinese, key));
    }

    [Fact]
    public void ServiceSwitchesLanguage()
    {
        var loc = new LocalizationService(AppLanguage.English);
        Assert.Equal("Settings", loc[TextKey.SettingsTitle]);
        loc.SetLanguage(AppLanguage.Chinese);
        Assert.Equal("设置", loc[TextKey.SettingsTitle]);
        Assert.Equal("6 秒", loc.Format(TextKey.Seconds, 6));
    }

    [Fact]
    public void ChineseCultureDefaultsToChinese()
    {
        Assert.Equal(AppLanguage.Chinese, AppLanguageExtensions.FromCulture(new CultureInfo("zh-CN")));
        Assert.Equal(AppLanguage.English, AppLanguageExtensions.FromCulture(new CultureInfo("en-US")));
        Assert.Equal(AppLanguage.Chinese, AppLanguageExtensions.FromCode("zh-Hans"));
        Assert.Equal("zh", AppLanguage.Chinese.ToCode());
    }
}

public class SettingsTests
{
    [Fact]
    public void RoundTripKeepsLanguageColorAndCamera()
    {
        var directory = Path.Combine(Path.GetTempPath(), "blinkmore-tests", Guid.NewGuid().ToString("n"));
        var path = Path.Combine(directory, "settings.json");
        var settings = UserSettings.CreateDefault(AppLanguage.Chinese);
        settings.FadeColorHex = "#e63333";
        settings.BlinkThresholdSeconds = 8.4;
        settings.FadeSpeedSeconds = 2.2;
        settings.Sensitivity = 0.22;
        settings.SelectedCameraId = "1";
        settings.EyeTrackingEnabled = true;
        settings.HasShownOnboarding = true;

        SettingsStore.Save(settings, path);
        var loaded = SettingsStore.Load(path);

        Assert.Equal(AppLanguage.Chinese, loaded.Language);
        Assert.Equal("#E63333", loaded.FadeColorHex);
        Assert.Equal(8, loaded.BlinkThresholdSeconds);
        Assert.Equal(2, loaded.FadeSpeedSeconds);
        Assert.Equal(SensitivityLevel.High, loaded.SensitivityLevel);
        Assert.Equal("1", loaded.SelectedCameraId);
        Assert.True(loaded.EyeTrackingEnabled);
        Assert.True(loaded.HasShownOnboarding);
        Assert.Equal(Accelerator.Cpu, loaded.Accelerator);
    }

    [Fact]
    public void GpuChoiceIsSaved()
    {
        var path = Path.Combine(Path.GetTempPath(), "blinkmore-tests", Guid.NewGuid().ToString("n"), "settings.json");
        var settings = UserSettings.CreateDefault(AppLanguage.English);
        settings.Accelerator = Accelerator.Gpu;
        SettingsStore.Save(settings, path);
        Assert.Equal(Accelerator.Gpu, SettingsStore.Load(path).Accelerator);
        Assert.Equal(Accelerator.Cpu, IntelGraphics.Parse("nope"));
    }

    [Fact]
    public void CorruptFileFallsBackToDefaults()
    {
        var path = Path.Combine(Path.GetTempPath(), "blinkmore-tests", Guid.NewGuid().ToString("n") + ".json");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, "{ this is not json");
        var loaded = SettingsStore.Load(path);
        Assert.InRange(loaded.BlinkThresholdSeconds, AppConstants.MinBlinkThreshold, AppConstants.MaxBlinkThreshold);
        Assert.False(loaded.HasShownOnboarding);
    }

    [Theory]
    [InlineData("#fff", "#FFFFFF")]
    [InlineData("#E63333", "#E63333")]
    [InlineData("nope", "#000000")]
    [InlineData("#123456", "#123456")]
    public void ColorsNormalize(string input, string expected)
    {
        Assert.Equal(expected, AppConstants.NormalizeColor(input));
    }
}

public class SensitivityTests
{
    [Theory]
    [InlineData(0.10, SensitivityLevel.Low)]
    [InlineData(0.16, SensitivityLevel.Medium)]
    [InlineData(0.22, SensitivityLevel.High)]
    public void MacSliderValuesMapToThreeSteps(double value, SensitivityLevel level)
    {
        Assert.Equal(level, SensitivityMap.FromValue(value));
        Assert.Equal(value, SensitivityMap.ToValue(level), 2);
    }

    [Fact]
    public void HigherSensitivityAsksForClearerEyes()
    {
        var low = BlinkClassifier.TuningFor(SensitivityLevel.Low);
        var medium = BlinkClassifier.TuningFor(SensitivityLevel.Medium);
        var high = BlinkClassifier.TuningFor(SensitivityLevel.High);
        Assert.True(low.MinNeighbors < medium.MinNeighbors);
        Assert.True(medium.MinNeighbors < high.MinNeighbors);
        Assert.True(low.MinEyeSize < high.MinEyeSize);
    }
}

public class ClassifierTests
{
    [Theory]
    [InlineData(0, 2, SensitivityLevel.Medium, EyeObservation.Absent)]
    [InlineData(2, 2, SensitivityLevel.Medium, EyeObservation.Absent)]
    [InlineData(1, 0, SensitivityLevel.Low, EyeObservation.Closed)]
    [InlineData(1, 1, SensitivityLevel.Low, EyeObservation.Open)]
    [InlineData(1, 1, SensitivityLevel.Medium, EyeObservation.Closed)]
    [InlineData(1, 1, SensitivityLevel.High, EyeObservation.Closed)]
    [InlineData(1, 2, SensitivityLevel.High, EyeObservation.Open)]
    public void ClassifiesFacesAndEyes(int faces, int eyes, SensitivityLevel sensitivity, EyeObservation expected)
    {
        Assert.Equal(expected, BlinkClassifier.Classify(faces, eyes, sensitivity));
    }
}

public class StabilizerTests
{
    [Fact]
    public void OneNoisyFrameDoesNotFlip()
    {
        var stabilizer = new EyeStateStabilizer(2);
        Assert.Equal(EyeObservation.Open, stabilizer.Push(EyeObservation.Open));
        Assert.Equal(EyeObservation.Open, stabilizer.Push(EyeObservation.Open));
        Assert.Equal(EyeObservation.Open, stabilizer.Push(EyeObservation.Closed));
        Assert.Equal(EyeObservation.Closed, stabilizer.Push(EyeObservation.Closed));
    }
}

public class FadeMonitorTests
{
    private static readonly DateTime Start = new(2026, 10, 8, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void FadeStartsWhenEyesStayOpenPastTheThreshold()
    {
        var monitor = NewMonitor();
        Assert.Equal(FadeTransition.None, monitor.Tick(Start, EyeObservation.Open).Transition);
        Assert.False(monitor.IsFaded);
        var decision = monitor.Tick(Start.AddSeconds(6), EyeObservation.Open);
        Assert.Equal(FadeTransition.Applied, decision.Transition);
        Assert.True(decision.IsFaded);
    }

    [Fact]
    public void BlinkRemovesTheFadeAndRestartsTheClock()
    {
        var monitor = NewMonitor();
        monitor.Tick(Start, EyeObservation.Open);
        monitor.Tick(Start.AddSeconds(6), EyeObservation.Open);
        var removed = monitor.Tick(Start.AddSeconds(7), EyeObservation.Closed);
        Assert.Equal(FadeTransition.Removed, removed.Transition);
        Assert.False(monitor.IsFaded);

        Assert.Equal(FadeTransition.None, monitor.Tick(Start.AddSeconds(8), EyeObservation.Open).Transition);
        Assert.Equal(FadeTransition.Applied, monitor.Tick(Start.AddSeconds(14), EyeObservation.Open).Transition);
    }

    [Fact]
    public void MissingFaceAlsoClearsTheFade()
    {
        var monitor = NewMonitor();
        monitor.Tick(Start, EyeObservation.Open);
        monitor.Tick(Start.AddSeconds(6), EyeObservation.Open);
        Assert.Equal(FadeTransition.Removed, monitor.Tick(Start.AddSeconds(6.2), EyeObservation.Absent).Transition);
    }

    [Fact]
    public void ContinuousFadeTurnsTrackingOff()
    {
        var monitor = NewMonitor();
        monitor.Tick(Start, EyeObservation.Open);
        monitor.Tick(Start.AddSeconds(6), EyeObservation.Open);
        var timedOut = monitor.Tick(Start.AddSeconds(12), EyeObservation.Open);
        Assert.Equal(FadeTransition.TimedOut, timedOut.Transition);
        Assert.True(timedOut.DisableEyeTracking);
        Assert.False(monitor.EyeTrackingEnabled);
        Assert.False(monitor.IsFaded);

        var after = monitor.Tick(Start.AddSeconds(20), EyeObservation.Open);
        Assert.Equal(FadeTransition.None, after.Transition);
        Assert.False(after.IsFaded);
    }

    [Fact]
    public void LoweringTheThresholdFadesOnTheNextTick()
    {
        var monitor = NewMonitor();
        monitor.Tick(Start, EyeObservation.Open);
        monitor.Tick(Start.AddSeconds(4), EyeObservation.Open);
        monitor.BlinkThreshold = TimeSpan.FromSeconds(3);
        Assert.Equal(FadeTransition.Applied, monitor.Tick(Start.AddSeconds(4), EyeObservation.Open).Transition);
    }

    private static FadeMonitor NewMonitor()
    {
        return new FadeMonitor
        {
            EyeTrackingEnabled = true,
            BlinkThreshold = TimeSpan.FromSeconds(6),
            FadeTimeout = TimeSpan.FromSeconds(6),
        };
    }
}

public class IntelGraphicsTests
{
    [Fact]
    public void PrefersIrisXeOverUhdAndIgnoresOtherVendors()
    {
        var devices = new[]
        {
            new OpenClGpu("NVIDIA CUDA", "NVIDIA", "NVIDIA GeForce RTX 3060"),
            new OpenClGpu("Intel(R) OpenCL", "Intel", "Intel(R) UHD Graphics 630"),
            new OpenClGpu("Intel(R) OpenCL HD Graphics", "Intel(R) Corporation", "Intel(R) Iris(R) Xe Graphics"),
        };

        var picked = IntelGraphics.Prefer(devices);
        Assert.NotNull(picked);
        Assert.Contains("Iris", picked!.Device);
        Assert.Equal("Intel:GPU:Iris", IntelGraphics.OpenCvDeviceVariable(picked));
    }

    [Fact]
    public void NoIntelGpuMeansCpuStaysInCharge()
    {
        var devices = new[]
        {
            new OpenClGpu("NVIDIA CUDA", "NVIDIA", "NVIDIA GeForce"),
        };
        Assert.Null(IntelGraphics.Prefer(devices));
    }
}

public class LetterboxTests
{
    [Fact]
    public void WebcamFrameKeepsItsShapeInsideTheModelSquare()
    {
        var box = Letterbox.Fit(640, 480, 320);
        Assert.Equal(320, box.Width);
        Assert.Equal(240, box.Height);
        Assert.Equal(0, box.X);
        Assert.Equal(40, box.Y);

        var mapped = box.TryMapToSource(10, 50, 100, 80, 640, 480, out var x, out var y, out var width, out var height);
        Assert.True(mapped);
        Assert.Equal(20, x);
        Assert.Equal(20, y);
        Assert.Equal(200, width);
        Assert.Equal(160, height);
    }
}

public class FontCoverageTests
{
    [Fact]
    public void EmbeddedFontContainsEveryLocalizedCharacter()
    {
        var fontPath = FindFont();
        var glyphs = TrueTypeCmap.ReadCharacters(fontPath);
        var missing = new List<string>();
        foreach (var key in TextKey.All)
        {
            foreach (var language in new[] { AppLanguage.English, AppLanguage.Chinese })
            {
                foreach (var character in TextCatalog.Get(language, key))
                {
                    if (char.IsWhiteSpace(character) || glyphs.Contains(character))
                        continue;
                    missing.Add($"{language}:{key}:{character}");
                }
            }
        }

        Assert.True(missing.Count == 0, "Font is missing " + string.Join(", ", missing.Distinct()));
    }

    private static string FindFont()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            var candidate = Path.Combine(dir.FullName, "src", "BlinkMore.App", "Assets", "Fonts", "NotoSansSC-Regular.ttf");
            if (File.Exists(candidate))
                return candidate;
            dir = dir.Parent;
        }

        throw new FileNotFoundException("NotoSansSC-Regular.ttf was not found. Run windows/tools/subset-font.py.");
    }
}

internal static class TrueTypeCmap
{
    public static HashSet<char> ReadCharacters(string path)
    {
        using var stream = File.OpenRead(path);
        using var reader = new BinaryReader(stream);
        var scaler = ReadUInt32(reader);
        if (scaler != 0x00010000 && scaler != 0x4F54544F)
            throw new InvalidDataException("Not a TrueType font.");
        var tableCount = ReadUInt16(reader);
        stream.Seek(6, SeekOrigin.Current);
        long cmapOffset = -1;
        uint cmapLength = 0;
        for (var i = 0; i < tableCount; i++)
        {
            var tag = System.Text.Encoding.ASCII.GetString(reader.ReadBytes(4));
            stream.Seek(4, SeekOrigin.Current);
            var offset = ReadUInt32(reader);
            var length = ReadUInt32(reader);
            if (tag == "cmap")
            {
                cmapOffset = offset;
                cmapLength = length;
            }
        }

        if (cmapOffset < 0)
            throw new InvalidDataException("cmap table is missing.");

        stream.Position = cmapOffset;
        stream.Seek(2, SeekOrigin.Current);
        var subtables = ReadUInt16(reader);
        var candidates = new List<(int platform, int encoding, uint offset)>();
        for (var i = 0; i < subtables; i++)
        {
            var platform = ReadUInt16(reader);
            var encoding = ReadUInt16(reader);
            var offset = ReadUInt32(reader);
            candidates.Add((platform, encoding, offset));
        }

        var chosen = candidates
            .OrderByDescending(item => item.platform == 3 && (item.encoding == 10 || item.encoding == 1))
            .ThenByDescending(item => item.platform == 0)
            .First();
        stream.Position = cmapOffset + chosen.offset;
        var format = ReadUInt16(reader);
        return format switch
        {
            4 => ReadFormat4(stream, reader),
            12 => ReadFormat12(stream, reader),
            _ => throw new InvalidDataException($"Unsupported cmap format {format}."),
        };
    }

    private static HashSet<char> ReadFormat4(Stream stream, BinaryReader reader)
    {
        stream.Seek(4, SeekOrigin.Current);
        var segCount = ReadUInt16(reader) / 2;
        stream.Seek(6, SeekOrigin.Current);
        var endCode = ReadUShortArray(reader, segCount);
        stream.Seek(2, SeekOrigin.Current);
        var startCode = ReadUShortArray(reader, segCount);
        var idDelta = ReadUShortArray(reader, segCount);
        var idRangeOffset = ReadUShortArray(reader, segCount);
        var glyphArrayStart = stream.Position;
        var glyphs = new HashSet<char>();
        for (var i = 0; i < segCount; i++)
        {
            for (var code = startCode[i]; code <= endCode[i] && code != 0xFFFF; code++)
            {
                int glyphId;
                if (idRangeOffset[i] == 0)
                {
                    glyphId = (code + idDelta[i]) & 0xFFFF;
                }
                else
                {
                    var offset = glyphArrayStart
                        + ((idRangeOffset.Length - i - 1) * 0)
                        + (idRangeOffset[i] + (code - startCode[i]) * 2);
                    // idRangeOffset is measured from its own entry.
                    var fromEntry = (cmapRangeOffsetPosition(i, segCount, glyphArrayStart)) + idRangeOffset[i] + ((code - startCode[i]) * 2);
                    stream.Position = fromEntry;
                    glyphId = ReadUInt16(reader);
                    if (glyphId != 0)
                        glyphId = (glyphId + idDelta[i]) & 0xFFFF;
                }

                if (glyphId != 0 && code <= char.MaxValue)
                    glyphs.Add((char)code);
            }
        }

        return glyphs;
    }

    private static long cmapRangeOffsetPosition(int index, int segCount, long glyphArrayStart)
    {
        // start of idRangeOffset array is glyphArrayStart - segCount * 2.
        return glyphArrayStart - (segCount * 2) + (index * 2);
    }

    private static HashSet<char> ReadFormat12(Stream stream, BinaryReader reader)
    {
        stream.Seek(10, SeekOrigin.Current);
        var groups = ReadUInt32(reader);
        var glyphs = new HashSet<char>();
        for (var i = 0; i < groups; i++)
        {
            var start = ReadUInt32(reader);
            var end = ReadUInt32(reader);
            stream.Seek(4, SeekOrigin.Current);
            for (var code = start; code <= end && code <= char.MaxValue; code++)
                glyphs.Add((char)code);
        }

        return glyphs;
    }

    private static ushort[] ReadUShortArray(BinaryReader reader, int count)
    {
        var values = new ushort[count];
        for (var i = 0; i < count; i++)
            values[i] = ReadUInt16(reader);
        return values;
    }

    private static ushort ReadUInt16(BinaryReader reader)
    {
        var bytes = reader.ReadBytes(2);
        return (ushort)((bytes[0] << 8) | bytes[1]);
    }

    private static uint ReadUInt32(BinaryReader reader)
    {
        var bytes = reader.ReadBytes(4);
        return (uint)((bytes[0] << 24) | (bytes[1] << 16) | (bytes[2] << 8) | bytes[3]);
    }
}
