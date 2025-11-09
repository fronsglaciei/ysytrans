using BepInEx;
using FG.Mods.YSYard.Translations.Helpers;
using Il2CppInterop.Runtime.Injection;
using System;
using System.Threading;
using UnityEngine;

namespace FG.Mods.YSYard.Translations.Services;

internal static class ExGameSettingManager
{
    private static readonly ReaderWriterLockSlim _lock = new();

    private static bool _onQuitCalled;

    internal static void SetAppQuitWatcher(GameObject rootObject)
    {
        if (rootObject == null)
        {
            return;
        }

        if (rootObject.GetComponent<AppQuitWatcher>() == null)
        {
            var watcher = rootObject.AddComponent<AppQuitWatcher>();
            watcher.hideFlags = HideFlags.HideAndDontSave;
            watcher.Callback = (Il2CppSystem.Action)OnAppQuit;
        }

        if (ConsoleManager.ConsoleEnabled)
        {
            ConsoleHelper.RegisterOnConsoleQuit(OnAppQuit);
        }
    }

    internal static void LoadLanguageSetting()
        //=> GameSettingManager.Instance.SetLanguage(
        //    (GameSettingManager.LanguageEnum)ConfigProvider.LastUsedLanguage.Value);
        => ds.bgsn.daq((ds.dr)ConfigProvider.LastUsedLanguage.Value);

    private static void OnAppQuit()
    {
        if (_onQuitCalled)
        {
            return;
        }

        using var _ = new WriteLock(_lock);

        //var curLang = GameSettingManager.Instance.GameLanguage;
        var curLang = ds.bgsn.vju;

        ConfigProvider.LastUsedLanguage.Value = (int)curLang;

        //if (curLang != GameSettingManager.LanguageEnum.SIMPLECHINESE
        //    && curLang != GameSettingManager.LanguageEnum.ENGILISH)
        if (curLang != ds.dr.SIMPLECHINESE
            && curLang != ds.dr.ENGILISH)
        {
            //GameSettingManager.Instance.SetLanguage(
            //    GameSettingManager.LanguageEnum.SIMPLECHINESE);
            ds.bgsn.daq(ds.dr.SIMPLECHINESE);
        }

        _onQuitCalled = true;
    }

    private class AppQuitWatcher : MonoBehaviour
    {
        internal Il2CppSystem.Action Callback { get; set; }

        static AppQuitWatcher()
        {
            ClassInjector.RegisterTypeInIl2Cpp<AppQuitWatcher>();
        }

        private void OnApplicationQuit()
            => this.Callback?.Invoke();
    }

    private class WriteLock : IDisposable
    {
        private readonly ReaderWriterLockSlim _objLock;

        internal WriteLock(ReaderWriterLockSlim objLock)
        {
            this._objLock = objLock;
            objLock.EnterWriteLock();
        }

        public void Dispose() => this._objLock.ExitWriteLock();
    }
}
