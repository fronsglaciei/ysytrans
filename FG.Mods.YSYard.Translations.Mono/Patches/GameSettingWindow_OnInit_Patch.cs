using Foundation.UI;
using HarmonyLib;

namespace FG.Mods.YSYard.Translations.Patches
{
    [HarmonyPatch(typeof(GameSettingWindow), nameof(GameSettingWindow.OnInit))]
    public static class GameSettingWindow_OnInit_Patch
    {
        public static void Postfix(GameSettingWindow __instance)
        {
            __instance.Japanese.gameObject.SetActive(true);
        }
    }
}
