namespace BlinkMore.Core;

public enum Accelerator
{
    Cpu,
    Gpu,
}

public sealed record OpenClGpu(string Platform, string Vendor, string Device);

public readonly record struct AcceleratorReport(bool UsingGpu, string? DeviceName, string? FailureDetail)
{
    public static AcceleratorReport Cpu() => new(false, null, null);

    public static AcceleratorReport Missing() => new(false, null, null);

    public static AcceleratorReport Ready(string deviceName) => new(true, deviceName, null);

    public static AcceleratorReport Failed(string detail) => new(false, null, detail);
}

public static class IntelGraphics
{
    public static OpenClGpu? Prefer(IReadOnlyList<OpenClGpu> devices)
    {
        var intel = devices.Where(IsIntel).ToList();
        if (intel.Count == 0)
            return null;

        foreach (var token in new[] { "Iris", "Arc", "Xe", "UHD", "HD Graphics" })
        {
            var match = intel.FirstOrDefault(device =>
                device.Device.Contains(token, StringComparison.OrdinalIgnoreCase));
            if (match != null)
                return match;
        }

        return intel[0];
    }

    /// <summary>
    /// OpenCV reads this from OPENCV_OPENCL_DEVICE: platform, GPU type, then a device-name fragment.
    /// "Iris" selects Iris Xe ahead of a UHD adapter on the same Intel platform.
    /// </summary>
    public static string OpenCvDeviceVariable(OpenClGpu gpu)
    {
        foreach (var token in new[] { "Iris", "Arc", "Xe", "UHD", "HD Graphics" })
        {
            if (gpu.Device.Contains(token, StringComparison.OrdinalIgnoreCase))
                return "Intel:GPU:" + token;
        }

        return "Intel:GPU:";
    }

    public static Accelerator Parse(string? value)
        => string.Equals(value, "gpu", StringComparison.OrdinalIgnoreCase)
            ? Accelerator.Gpu
            : Accelerator.Cpu;

    public static string ToCode(Accelerator accelerator)
        => accelerator == Accelerator.Gpu ? "gpu" : "cpu";

    private static bool IsIntel(OpenClGpu device)
        => ContainsIntel(device.Platform) || ContainsIntel(device.Vendor) || ContainsIntel(device.Device);

    private static bool ContainsIntel(string value)
        => value.Contains("Intel", StringComparison.OrdinalIgnoreCase);
}
