using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using FG.Mods.YSYard.Translations.Services;
using HarmonyLib;
using System.IO;
using System.Reflection;

namespace FG.Mods.YSYard.Translations;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BasePlugin
{
    internal static new ManualLogSource Log;

    public override void Load()
    {
        Log = base.Log;

        ConfigProvider.Init(this.Config);

        var assembly = Assembly.GetExecutingAssembly();
        PathProvider.Init(Path.GetDirectoryName(assembly.Location));

        TranslationProvider.LoadTranslations();

        Harmony.CreateAndPatchAll(assembly);
    }
}
