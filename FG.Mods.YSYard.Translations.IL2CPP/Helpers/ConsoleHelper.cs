using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace FG.Mods.YSYard.Translations.Helpers;

internal static class ConsoleHelper
{
    private static readonly List<Action> _onConsoleQuitCallbacks = [];

    private static readonly NativeMethods.HandlerRoutine _conMsgHandler
        = new(OnConsoleWindowMessage);

    private static bool _conMsgHandlerSet;

    internal static void RegisterOnConsoleQuit(Action onConsoleQuit)
    {
        if (onConsoleQuit == null)
        {
            return;
        }
        _onConsoleQuitCallbacks.Add(onConsoleQuit);

        if (_conMsgHandlerSet)
        {
            return;
        }
        _conMsgHandlerSet = NativeMethods
            .SetConsoleCtrlHandler(_conMsgHandler, true);
        if (!_conMsgHandlerSet)
        {
            Plugin.Log.LogWarning("Failed to SetConsoleCtrlHandler");
        }
    }

    private static bool OnConsoleWindowMessage(uint msg)
    {
        switch (msg)
        {
            case 2:
            case 5:
            case 6:
                foreach (var callback in _onConsoleQuitCallbacks)
                {
                    callback.Invoke();
                }
                break;
        }
        return false;
    }

    private static class NativeMethods
    {
        [return: MarshalAs(UnmanagedType.Bool)]
        public delegate bool HandlerRoutine(uint dwCtrlType);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetConsoleCtrlHandler(
            HandlerRoutine HandlerRoutine,
            [MarshalAs(UnmanagedType.Bool)] bool Add);
    }
}
