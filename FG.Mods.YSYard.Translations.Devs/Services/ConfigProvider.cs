using BepInEx.Configuration;

namespace FG.Mods.YSYard.Translations.Devs.Services;

public static class ConfigProvider
{
    public static ConfigEntry<bool> NotifiesKey { get; private set; }

    public static ConfigEntry<bool> ShowsExportButton { get; private set; }

    internal static void Init(ConfigFile configFile)
    {
        NotifiesKey = configFile.Bind(
            "General",
            nameof(NotifiesKey),
            false,
            "If true, this mod notifies Language keys requested on gameplay via MemoryMappedFile.\nThis option is supposed to be used by a translator.");

        ShowsExportButton = configFile.Bind(
            "General",
            nameof(ShowsExportButton),
            false,
            "If true, this mod shows Export-Button that exports Language assets as JSON.\nThis option is supposed to be used by a translator.");
    }
}
