using FG.Defs.YSYard.Translations.Devs;

namespace FG.Mods.YSYard.Translations.Devs.Services;

public static class PathProvider
{
    public static DevelopmentPathDefs PathDef { get; private set; }

    internal static void Init(string rootPath)
    {
        PathDef = new DevelopmentPathDefs(rootPath);
        if (PathDef.IsValid)
        {
            PathDef.EnsureAllCreated();
        }
    }
}
