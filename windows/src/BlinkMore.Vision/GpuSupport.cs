using BlinkMore.Core;

namespace BlinkMore.Vision;

public static class GpuSupport
{
    public static AcceleratorReport Probe()
    {
        OpenClGpu? gpu;
        try
        {
            gpu = IntelGraphics.Prefer(OpenClCatalog.List());
        }
        catch (Exception ex)
        {
            return AcceleratorReport.Failed(ex.Message);
        }

        if (gpu == null)
            return AcceleratorReport.Missing();

        try
        {
            using var detector = YunetFaceDetector.CreateForIntelGpu(gpu);
            return AcceleratorReport.Ready(gpu.Device);
        }
        catch (Exception ex)
        {
            return AcceleratorReport.Failed(ex.Message);
        }
    }
}
