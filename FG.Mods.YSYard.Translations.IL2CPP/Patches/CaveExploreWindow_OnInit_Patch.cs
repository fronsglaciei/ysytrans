using FG.Mods.YSYard.Translations.Services;
using Foundation.UI;
using HarmonyLib;
using UnityEngine.UI;

namespace FG.Mods.YSYard.Translations.Patches;

[HarmonyPatch(typeof(CaveExploreWindow), nameof(CaveExploreWindow.OnInit))]
public static class CaveExploreWindow_OnInit_Patch
{
    public static void Postfix(CaveExploreWindow __instance)
    {
        var root = __instance.mUIData?.transform;
        var levelMax = __instance.LevelMax?.GetComponent<Text>();
        if (root == null || levelMax == null)
        {
            return;
        }

        var uts = root.GetComponent<UITextSetting>();
        if (uts == null || uts.textDatas == null)
        {
            return;
        }
        uts.textDatas.Add(new(levelMax, TranslationProvider.KEY_MINING_LEVEL_MAX));
    }
}
