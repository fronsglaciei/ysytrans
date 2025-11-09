using Foundation.UI;
using HarmonyLib;
using System.Collections.Generic;

namespace FG.Mods.YSYard.Translations.Patches
{
    [HarmonyPatch(typeof(bbg), nameof(bbg.cnm))]
    public static class CleanupWindow_OnInit_Patch
    {
        private static readonly Dictionary<string, int> _textKeyMap = new()
        {
            ["打扫卫生"] = 13102420,
            ["初级打扫"] = 13102421,
            ["中级清扫"] = 13102422,
            ["高级清理"] = 13102423,
            ["整洁度"] = 13102424,
            ["San"] = 13102425,
            ["开始打扫"] = 13102426
        };

        public static void Postfix(bbg __instance)
        {
            //var textSetting = __instance.mGameObject.GetComponent<UITextSetting>();
            var textSetting = __instance.zqb.GetComponent<UITextSetting>();
            if (textSetting == null)
            {
                return;
            }

            // fix TextData whose key is not set correctly
            foreach (var textData in textSetting.textDatas)
            {
                if (_textKeyMap.TryGetValue(textData.text.text, out var key))
                {
                    textData.languageID = key;
                }
            }
        }
    }
}
