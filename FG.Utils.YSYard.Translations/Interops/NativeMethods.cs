using System.Runtime.InteropServices;

namespace FG.Utils.YSYard.Translations.Interops;

public static class NativeMethods
{
    [DllImport("shlwapi.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    private static extern int StrCmpLogicalW(string x, string y);

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

    public static int LogicalCompare(string x, string y) => StrCmpLogicalW(x, y);

    private const int BUILD_VERSION_17763 = 17763;
    private const int BUILD_VERSION_18985 = 18985;

    private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
    private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

    public static bool UseImmersiveDarkMode(IntPtr hWnd, bool enabled)
    {
        if (IsWindows10OrGreater(BUILD_VERSION_17763))
        {
            var attribute = DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1;
            if (IsWindows10OrGreater(BUILD_VERSION_18985))
            {
                attribute = DWMWA_USE_IMMERSIVE_DARK_MODE;
            }

            int useImmersiveDarkMode = enabled ? 1 : 0;
            return DwmSetWindowAttribute(hWnd, attribute, ref useImmersiveDarkMode, sizeof(int)) == 0;
        }

        return false;
    }

    private static bool IsWindows10OrGreater(int build = -1)
    {
        return Environment.OSVersion.Version.Major >= 10 && Environment.OSVersion.Version.Build >= build;
    }
}
