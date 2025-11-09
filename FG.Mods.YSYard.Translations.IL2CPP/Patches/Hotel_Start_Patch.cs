using FG.Mods.YSYard.Translations.Services;
using HarmonyLib;
using HotelModule;

namespace FG.Mods.YSYard.Translations.Patches;

[HarmonyPatch(typeof(Hotel), nameof(Hotel.Start))]
public static class Hotel_Start_Patch
{
    public static void Prefix(Hotel __instance)
    {
        ExGameSettingManager.SetAppQuitWatcher(__instance.gameObject);
        ExGameSettingManager.LoadLanguageSetting();
    }
}
