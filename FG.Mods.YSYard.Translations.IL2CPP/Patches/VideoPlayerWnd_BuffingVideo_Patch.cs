using FG.Mods.YSYard.Translations.Services;
using HarmonyLib;

namespace FG.Mods.YSYard.Translations.Patches;

[HarmonyPatch(typeof(iz), nameof(iz.ehr))]
public static class VideoPlayerWnd_BuffingVideo_Patch
{
    public static void Postfix(string a, iz __instance)
    {
        SubtitleManager.SetSrt(__instance, a);
    }
}
