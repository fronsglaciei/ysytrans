using DG.Tweening;
using FG.Mods.YSYard.Translations.Services;
using Foundation.UI;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace FG.Mods.YSYard.Translations.Patches;

[HarmonyPatch(typeof(StartPageWindow), nameof(StartPageWindow.OnInit))]
public static class StartPageWindow_OnInit_Patch
{
    private const string BUTTON_NAME_EXPORT_RESOURCES = "ExportResources";

    private const string BUTTON_TIPS_EXPORT_RESOURCES = "Export Language resources";

    private const string BUTTON_NAME_EXPORT_STORIES = "ExportStories";

    private const string BUTTON_TIPS_EXPORT_STORIES = "Export Stories";

    private const string BUTTON_NAME_RELOAD_TRANSLATIONS = "ReloadTranslations";

    private const string BUTTON_TIPS_RELOAD_TRANSLATIONS = "Reload Translations";

    public static void Postfix(StartPageWindow __instance)
    {
        if (ConfigProvider.ShowsExportButton.Value)
        {
            CloneButton(
                __instance,
                BUTTON_NAME_EXPORT_RESOURCES,
                BUTTON_TIPS_EXPORT_RESOURCES,
                LanguageResourceExporter.ExportLanguages);

            CloneButton(
                __instance,
                BUTTON_NAME_EXPORT_STORIES,
                BUTTON_TIPS_EXPORT_STORIES,
                LanguageResourceExporter.ExportStoryPlots);
        }

        if (ConfigProvider.ShowsReloadTranslationsButton.Value)
        {
            CloneButton(
                __instance,
                BUTTON_NAME_RELOAD_TRANSLATIONS,
                BUTTON_TIPS_RELOAD_TRANSLATIONS,
                TranslationProvider.LoadTranslations);
        }
    }

    public static void CloneButton(StartPageWindow window, string objName, string tips, System.Action onClick)
    {
        // lifts up VerticalLayoutGroup
        var parent = window.Discord.transform.parent;
        var rtParent = parent.GetComponent<RectTransform>();
        rtParent.localPosition += new Vector3(0f, 40f, 0f);

        // clone
        var obj = GameObject.Instantiate(window.Discord);

        obj.name = objName;
        GameObject.Destroy(obj.GetComponent<DOTweenAnimation>());

        // add cloned object to VerticalLayoutGroup
        obj.transform.SetParent(window.Discord.transform.parent, false);

        var rt = obj.GetComponent<RectTransform>();
        rt.localPosition = new Vector3(rt.localPosition.x, rt.localPosition.y - 20f, 0);

        var img = obj.GetComponent<Image>();
        img.color = new Color(img.color.r, img.color.g, img.color.b, 1f);

        var mie = obj.GetComponent<MouseInEvent>();
        Il2CppSystem.Action onPointerEnter = new System.Action(() =>
        {
            var tmpRt = obj.GetComponent<RectTransform>();
            var tipsRt = window.TipsBG.GetComponent<RectTransform>();
            tipsRt.anchoredPosition = tmpRt.anchoredPosition;
            tipsRt.anchoredPosition += new Vector2(-20f, 0f);
            window.Tips.text = tips;
            window.TipsBG.gameObject.SetActive(true);
        });
        mie.mPointEnterCallback = onPointerEnter;
        Il2CppSystem.Action onPointerExit = new System.Action(() =>
        {
            window.TipsBG.gameObject.SetActive(false);
        });
        mie.mPointExitCallback = onPointerExit;

        var btn = obj.GetComponent<Button>();
        btn.onClick.AddListener((UnityAction)onClick);
    }
}
