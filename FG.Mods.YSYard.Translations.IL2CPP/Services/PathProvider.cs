using FG.Defs.YSYard.Translations;

namespace FG.Mods.YSYard.Translations.Services;

public static class PathProvider
{
    public static LanguagePathDefs PathDef { get; private set; }

    internal static void Init(string rootPath)
    {
        PathDef = new LanguagePathDefs(rootPath);
        if (PathDef.IsValid)
        {
            PathDef.EnsureAllCreated();
        }
    }
}
