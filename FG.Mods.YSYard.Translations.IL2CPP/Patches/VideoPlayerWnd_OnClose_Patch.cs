using FG.Mods.YSYard.Translations.Services;
using HarmonyLib;

namespace FG.Mods.YSYard.Translations.Patches;

[HarmonyPatch(typeof(VideoPlayerWnd), nameof(VideoPlayerWnd.OnClose))]
public static class VideoPlayerWnd_OnClose_Patch
{
    public static void Postfix(VideoPlayerWnd __instance)
    {
        SubtitleManager.ClearSrt(__instance);
    }
}
