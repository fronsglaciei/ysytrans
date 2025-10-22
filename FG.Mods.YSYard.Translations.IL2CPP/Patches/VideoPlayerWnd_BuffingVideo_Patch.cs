using FG.Mods.YSYard.Translations.Services;
using HarmonyLib;

namespace FG.Mods.YSYard.Translations.Patches;

[HarmonyPatch(typeof(VideoPlayerWnd), nameof(VideoPlayerWnd.BuffingVideo))]
public static class VideoPlayerWnd_BuffingVideo_Patch
{
    public static void Postfix(string path, VideoPlayerWnd __instance)
    {
        SubtitleManager.SetSrt(__instance, path);
    }
}
