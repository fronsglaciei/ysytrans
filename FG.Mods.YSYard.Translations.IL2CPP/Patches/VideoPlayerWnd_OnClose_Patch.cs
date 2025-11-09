using FG.Mods.YSYard.Translations.Services;
using HarmonyLib;

namespace FG.Mods.YSYard.Translations.Patches;

[HarmonyPatch(typeof(iz), nameof(iz.cnp))]
public static class VideoPlayerWnd_OnClose_Patch
{
    public static void Postfix(iz __instance)
    {
        SubtitleManager.ClearSrt(__instance);
    }
}
