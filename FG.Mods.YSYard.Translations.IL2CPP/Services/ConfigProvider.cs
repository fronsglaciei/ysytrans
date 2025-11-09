using BepInEx.Configuration;

namespace FG.Mods.YSYard.Translations.Services;

public static class ConfigProvider
{
    public static ConfigEntry<int> LastUsedLanguage { get; private set; }

    internal static void Init(ConfigFile configFile)
    {
        LastUsedLanguage = configFile.Bind(
            "General",
            nameof(LastUsedLanguage),
            0,
            "最後にセットしたゲーム内言語コードが保存されます.\n0: 中国語\n3: 英語\n1: 日本語");
    }
}
