using FG.Defs.YSYard.Translations.Devs;

namespace FG.Mods.YSYard.Translations.Devs.Services;

public static class KeyNotifier
{
    private static KeyNotificationServer<LanguageKey> _server = null;

    public static void StartServer()
    {
        _server?.Dispose();
        _server = new KeyNotificationServer<LanguageKey>();
    }

    public static void Notify(LanguageKey key) => _server?.Notify(key);

    public static void StopServer() => _server?.Dispose();
}
