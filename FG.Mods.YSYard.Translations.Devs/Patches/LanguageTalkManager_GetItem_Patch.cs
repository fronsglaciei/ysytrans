using Example;
using FG.Defs.YSYard.Translations.Devs;
using FG.Mods.YSYard.Translations.Devs.Services;
using HarmonyLib;

namespace FG.Mods.YSYard.Translations.Devs.Patches;

[HarmonyPatch(typeof(LanguageTalkManager), nameof(LanguageTalkManager.GetItem))]
public static class LanguageTalkManager_GetItem_Patch
{
    public static void Postfix(ref LanguageTalk __result, int key)
    {
        KeyNotifier.Notify(new KeyNotification
        {
            KeyType = LanguageKeyTypes.LanguageTalk,
            Key = key
        });
    }
}
