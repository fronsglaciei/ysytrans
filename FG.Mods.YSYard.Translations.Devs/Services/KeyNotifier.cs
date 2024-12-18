﻿using FG.Defs.YSYard.Translations.Devs;

namespace FG.Mods.YSYard.Translations.Devs.Services;

public static class KeyNotifier
{
    private static KeyNotificationServer _server = null;

    public static void StartServer()
    {
        _server?.Dispose();
        _server = new KeyNotificationServer();
    }

    public static void Notify(KeyNotification kn) => _server?.Notify(kn);

    public static void StopServer() => _server?.Dispose();
}
