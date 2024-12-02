using Example;
using FG.Mods.YSYard.Translations.Services;
using HarmonyLib;

namespace FG.Mods.YSYard.Translations.Patches;

[HarmonyPatch(typeof(LanguageTalkManager), nameof(LanguageTalkManager.GetItem))]
public static class LanguageTalkManager_GetItem_Patch
{
    public static void Postfix(ref LanguageTalk __result, int key)
    {
        if (__result != null && TranslationProvider.TryGetLanguageTalkTranslation(key, out var translation))
        {
            __result.LanguageJP = translation;
        }
    }
}
