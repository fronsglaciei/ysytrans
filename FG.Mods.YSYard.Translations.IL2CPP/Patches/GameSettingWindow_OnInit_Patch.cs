using Foundation.UI;
using HarmonyLib;

namespace FG.Mods.YSYard.Translations.Patches;

[HarmonyPatch(typeof(bfi), nameof(bfi.cnm))]
public static class GameSettingWindow_OnInit_Patch
{
    public static void Postfix(bfi __instance)
    {
        //__instance.Japanese.gameObject.SetActive(true);
        __instance.bclh.gameObject.SetActive(true);
        if (ds.bgsn.vju == ds.dr.JANPANESE)
        {
            __instance.bclh.SetIsOnWithoutNotify(true);
        }
    }
}
