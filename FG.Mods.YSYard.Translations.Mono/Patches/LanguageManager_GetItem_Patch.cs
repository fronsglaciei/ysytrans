using Example;
using FG.Defs.YSYard.Translations;
using FG.Mods.YSYard.Translations.Services;
using HarmonyLib;

namespace FG.Mods.YSYard.Translations.Patches
{
    [HarmonyPatch(typeof(LanguageManager), nameof(LanguageManager.GetItem))]
    public static class LanguageManager_GetItem_Patch
    {
        public static void Postfix(ref Language __result, int key)
        {
            KeyNotifier.Notify(new KeyNotification
            {
                KeyType = LanguageKeyTypes.Language,
                Key = key
            });
            if (__result != null && TranslationProvider.TryGetLanguageTranslation(key, out var translation))
            {
                __result.LanguageJpn = translation;
            }
        }
    }
}
