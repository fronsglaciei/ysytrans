namespace FG.Utils.YSYard.Translations.Helpers;

internal static class DictionaryExtensions
{
    internal static U GetOrCreateGet<T, U>(this Dictionary<T, U> dict, T key)
        where T : notnull
        where U : new()
    {
        if (dict.TryGetValue(key, out var val))
        {
            return val;
        }
        var ret = new U();
        dict[key] = ret;
        return ret;
    }
}
