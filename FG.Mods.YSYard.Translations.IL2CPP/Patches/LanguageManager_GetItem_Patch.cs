using Example;
using FG.Mods.YSYard.Translations.Services;
using HarmonyLib;

namespace FG.Mods.YSYard.Translations.Patches;

[HarmonyPatch(typeof(LanguageManager), nameof(LanguageManager.GetItem))]
public static class LanguageManager_GetItem_Patch
{
    public static void Postfix(ref Language __result, int key)
    {
        if (__result != null && TranslationProvider.TryGetLanguageTranslation(key, out var translation))
        {
            __result.LanguageJpn = translation;
        }
    }
}
