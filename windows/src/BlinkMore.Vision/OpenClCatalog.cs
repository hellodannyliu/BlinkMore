using System.Runtime.InteropServices;
using BlinkMore.Core;

namespace BlinkMore.Vision;

internal static class OpenClCatalog
{
    private const uint PlatformName = 0x0903;
    private const uint DeviceName = 0x102B;
    private const uint DeviceVendor = 0x102C;
    private const ulong DeviceTypeGpu = 4;
    private const int DeviceNotFound = -1;

    public static IReadOnlyList<OpenClGpu> List()
    {
        if (!TryLoad(out var library))
            return [];

        try
        {
            return Query(library);
        }
        catch (Exception)
        {
            return [];
        }
    }

    private static bool TryLoad(out IntPtr library)
    {
        if (NativeLibrary.TryLoad("OpenCL", out library))
            return true;
        if (OperatingSystem.IsWindows() && NativeLibrary.TryLoad("OpenCL.dll", out library))
            return true;
        return NativeLibrary.TryLoad("libOpenCL.so.1", out library);
    }

    private static List<OpenClGpu> Query(IntPtr library)
    {
        var getPlatforms = Load<GetPlatformIds>(library, "clGetPlatformIDs");
        var platformInfo = Load<GetInfo>(library, "clGetPlatformInfo");
        var getDevices = Load<GetDeviceIds>(library, "clGetDeviceIDs");
        var deviceInfo = Load<GetInfo>(library, "clGetDeviceInfo");

        var found = new List<OpenClGpu>();
        if (getPlatforms(0, IntPtr.Zero, out var platformCount) != 0 || platformCount == 0)
            return found;

        var platforms = Marshal.AllocHGlobal(IntPtr.Size * (int)platformCount);
        try
        {
            if (getPlatforms(platformCount, platforms, out platformCount) != 0)
                return found;

            for (var i = 0; i < platformCount; i++)
            {
                var platform = Marshal.ReadIntPtr(platforms, i * IntPtr.Size);
                var platformName = ReadString(platform, PlatformName, platformInfo);
                var deviceStatus = getDevices(platform, DeviceTypeGpu, 0, IntPtr.Zero, out var deviceCount);
                if (deviceStatus == DeviceNotFound || deviceCount == 0)
                    continue;

                var devices = Marshal.AllocHGlobal(IntPtr.Size * (int)deviceCount);
                try
                {
                    if (getDevices(platform, DeviceTypeGpu, deviceCount, devices, out deviceCount) != 0)
                        continue;

                    for (var deviceIndex = 0; deviceIndex < deviceCount; deviceIndex++)
                    {
                        var device = Marshal.ReadIntPtr(devices, deviceIndex * IntPtr.Size);
                        found.Add(new OpenClGpu(
                            platformName,
                            ReadString(device, DeviceVendor, deviceInfo),
                            ReadString(device, DeviceName, deviceInfo)));
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(devices);
                }
            }
        }
        finally
        {
            Marshal.FreeHGlobal(platforms);
        }

        return found;
    }

    private static string ReadString(IntPtr handle, uint parameter, GetInfo info)
    {
        if (info(handle, parameter, 0, IntPtr.Zero, out var size) != 0 || size == 0)
            return "";

        var buffer = Marshal.AllocHGlobal((int)size);
        try
        {
            if (info(handle, parameter, size, buffer, out _) != 0)
                return "";
            return Marshal.PtrToStringUTF8(buffer)?.TrimEnd('\0') ?? "";
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    private static T Load<T>(IntPtr library, string name) where T : Delegate
        => Marshal.GetDelegateForFunctionPointer<T>(NativeLibrary.GetExport(library, name));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int GetPlatformIds(uint numEntries, IntPtr platforms, out uint numPlatforms);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int GetDeviceIds(IntPtr platform, ulong deviceType, uint numEntries, IntPtr devices, out uint numDevices);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int GetInfo(IntPtr handle, uint parameter, nuint size, IntPtr value, out nuint sizeReturned);
}
