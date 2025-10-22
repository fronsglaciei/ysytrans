using FG.Mods.YSYard.Translations.Services;
using Foundation.UI;
using HarmonyLib;

namespace FG.Mods.YSYard.Translations.Patches;

[HarmonyPatch(typeof(AdventureWindow), nameof(AdventureWindow.OnInit))]
public static class AdventureWindow_OnInit_Patch
{
    public static void Postfix(AdventureWindow __instance)
    {
        var ep = __instance.mExplorationPageWindow;
        if (ep == null || ep.LevelMax == null)
        {
            return;
        }

        var uts = ep.mTransform?.GetComponent<UITextSetting>();
        if (uts == null || uts.textDatas == null)
        {
            return;
        }
        uts.textDatas.Add(new(ep.LevelMax, TranslationProvider.KEY_EXPLORATION_LEVEL_MAX));
    }
}
