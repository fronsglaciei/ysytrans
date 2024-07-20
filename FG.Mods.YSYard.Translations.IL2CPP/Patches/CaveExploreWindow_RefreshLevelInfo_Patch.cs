using Foundation.UI;
using HarmonyLib;
using UnityEngine.UI;

namespace FG.Mods.YSYard.Translations.Patches;

[HarmonyPatch(typeof(CaveExploreWindow), nameof(CaveExploreWindow.RefreshLevelInfo))]
public static class CaveExploreWindow_RefreshLevelInfo_Patch
{
    private static string _originalText = string.Empty;

    public static void Postfix(CaveExploreWindow __instance)
    {
        var levelMaxText = __instance.LevelMax.GetComponent<Text>();
        if (levelMaxText == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(_originalText))
        {
            _originalText = levelMaxText.text;
        }
        if (GameSettingManager.Instance.GameLanguage == GameSettingManager.LanguageEnum.JANPANESE)
        {
            levelMaxText.text = "最高レベルに到達";
        }
        else
        {
            levelMaxText.text = _originalText;
        }
    }
}
