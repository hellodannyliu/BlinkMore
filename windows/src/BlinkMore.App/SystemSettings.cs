using System.Diagnostics;

namespace BlinkMore;

internal static class SystemSettings
{
    public static void OpenCameraPrivacy()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = OperatingSystem.IsWindows()
                    ? "ms-settings:privacy-webcam"
                    : "https://support.microsoft.com/windows/camera-privacy-settings",
                UseShellExecute = true,
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex.Message);
        }
    }

    public static void OpenAuthorPage()
    {
        try
        {
            Process.Start(new ProcessStartInfo(BlinkMore.Core.AppConstants.AuthorUrl)
            {
                UseShellExecute = true,
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex.Message);
        }
    }
}
