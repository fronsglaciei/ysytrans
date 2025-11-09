using FG.Mods.YSYard.Translations.Services;
using HarmonyLib;

namespace FG.Mods.YSYard.Translations.Patches;

[HarmonyPatch(typeof(iz), nameof(iz.cnm))]
public static class VideoPlayerWnd_OnInit_Patch
{
    public static void Postfix(iz __instance)
    {
        SubtitleManager.OnVideoPlayerInit(__instance);
    }
}
