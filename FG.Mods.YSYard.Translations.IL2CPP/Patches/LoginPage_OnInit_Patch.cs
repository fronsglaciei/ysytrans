using FG.Mods.YSYard.Translations.Services;
using Foundation.UI;
using HarmonyLib;
using UnityEngine;

namespace FG.Mods.YSYard.Translations.Patches;

[HarmonyPatch(typeof(LoginPage), nameof(LoginPage.OnInit))]
public static class LoginPage_OnInit_Patch
{
    private const string KEY_LANGUAGE = "LanguageSettingStr";

    public static void Postfix(LoginPage __instance)
    {
        TranslationProvider.InjectExtraLanguages();

        // UITextSetting.SetText fails to set localized text
        // because GameAPI.GetLanguageStr cannot get correct language setting
        // before LoginPage is open
        GameSettingManager.Instance.GameLanguage =
            (GameSettingManager.LanguageEnum)PlayerPrefs.GetInt(KEY_LANGUAGE, 0);
    }
}
