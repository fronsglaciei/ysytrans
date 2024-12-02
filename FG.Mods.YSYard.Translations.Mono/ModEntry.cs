using BepInEx;
using BepInEx.Logging;
using FG.Mods.YSYard.Translations.Services;
using HarmonyLib;
using System.IO;
using System.Reflection;

namespace FG.Mods.YSYard.Translations
{
    [BepInPlugin(ModEntry.PLUGIN_ID, ModEntry.PLUGIN_NAME, ModEntry.PLUGIN_VERSION)]
    public class ModEntry : BaseUnityPlugin
    {
        public const string PLUGIN_ID = "FG.Mods.YSYard.Translations";

        public const string PLUGIN_NAME = "Unofficial Japanese Translation Mod";

        public const string PLUGIN_VERSION = "1.0.3";

        public static ManualLogSource RootLogger
        {
            get; private set;
        }

        public ModEntry()
        {
            ModEntry.RootLogger = this.Logger;

            var assembly = Assembly.GetExecutingAssembly();
            PathProvider.Init(Path.GetDirectoryName(assembly.Location));

            TranslationProvider.LoadTranslations();

            Harmony.CreateAndPatchAll(assembly);
        }
    }
}
