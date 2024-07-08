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
        public const string PLUGIN_ID = "fg.mods.ysyard.translations.jp";

        public const string PLUGIN_NAME = "Unofficial Japanese Translation Mod";

        public const string PLUGIN_VERSION = "1.0.1";

        public static ManualLogSource RootLogger
        {
            get; private set;
        }

        public ModEntry()
        {
            ModEntry.RootLogger = this.Logger;

            ConfigProvider.Init(this.Config);

            var assembly = Assembly.GetExecutingAssembly();
            PathProvider.Init(Path.GetDirectoryName(assembly.Location));

            if (ConfigProvider.NotifiesKey.Value)
            {
                KeyNotifier.StartServer();
            }

            TranslationProvider.LoadTranslations();

            Harmony.CreateAndPatchAll(assembly);
        }

        private void OnDestroy()
        {
            KeyNotifier.StopServer();
        }
    }
}
