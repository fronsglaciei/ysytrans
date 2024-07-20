using Foundation.UI;
using HarmonyLib;

namespace FG.Mods.YSYard.Translations.Patches;

[HarmonyPatch(typeof(ExplorationPageWindow), nameof(ExplorationPageWindow.SetPages))]
public static class ExplorationPageWindow_SetPages_Patch
{
    private static string _originalText = string.Empty;

    public static void Postfix(ExplorationPageWindow __instance)
    {
        if (string.IsNullOrEmpty(_originalText))
        {
            _originalText = __instance.LevelMax.text;
        }
        if (GameSettingManager.Instance.GameLanguage == GameSettingManager.LanguageEnum.JANPANESE)
        {
            __instance.LevelMax.text = "最高レベルに到達";
        }
        else
        {
            __instance.LevelMax.text = _originalText;
        }
    }
}
