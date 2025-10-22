using FG.Mods.YSYard.Translations.Services;
using HarmonyLib;

namespace FG.Mods.YSYard.Translations.Patches;

[HarmonyPatch(typeof(VideoPlayerWnd), nameof(VideoPlayerWnd.OnInit))]
public static class VideoPlayerWnd_OnInit_Patch
{
    public static void Postfix(VideoPlayerWnd __instance)
    {
        SubtitleManager.OnVideoPlayerInit(__instance);
    }
}
