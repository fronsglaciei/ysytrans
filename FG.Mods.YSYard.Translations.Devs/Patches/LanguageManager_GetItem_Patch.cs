using Example;
using FG.Defs.YSYard.Translations.Devs;
using FG.Mods.YSYard.Translations.Devs.Services;
using HarmonyLib;

namespace FG.Mods.YSYard.Translations.Devs.Patches;

[HarmonyPatch(typeof(LanguageManager), nameof(LanguageManager.GetItem))]
public static class LanguageManager_GetItem_Patch
{
    public static void Postfix(ref Language __result, int key)
    {
        KeyNotifier.Notify(new LanguageKey
        {
            KeyType = LanguageKeyTypes.Language,
            Key = key,
            TimeStamp = System.DateTime.Now,
        });
    }
}
