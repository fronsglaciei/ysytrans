using Foundation.UI;
using HarmonyLib;

namespace FG.Mods.YSYard.Translations.Patches
{
    [HarmonyPatch(typeof(SettlementMonthWindow), "BeforePlay")]
    public static class SettlementMonthWindow_BeforePlay_Patch
    {
        public static void Postfix(SettlementMonthWindow __instance)
        {
            __instance.ResultText.text = GameAPI.GetLanguageStr(13102355);
        }
    }
}
