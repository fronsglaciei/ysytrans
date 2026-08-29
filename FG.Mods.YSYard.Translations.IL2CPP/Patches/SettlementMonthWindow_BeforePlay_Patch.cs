using Foundation.UI;
using HarmonyLib;

namespace FG.Mods.YSYard.Translations.Patches;

[HarmonyPatch(typeof(bdj), nameof(bdj.kwy))]
public static class SettlementMonthWindow_BeforePlay_Patch
{
    public static void Postfix(bdj __instance)
    {
        //__instance.ResultText.text = GameAPI.GetLanguageStr(13102355);
        __instance.bbqo.text = cy.cus(13102355);
    }
}
