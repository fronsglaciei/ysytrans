namespace FG.Utils.YSYard.Translations.Helpers;

internal static class HashSetExtensions
{
    internal static
        (HashSet<T> onlyInSrc, HashSet<T> onlyInDst, HashSet<T> shared)
        DiffWith<T>(this HashSet<T> srcSet, HashSet<T> dstSet)
    {
        HashSet<T> dupSrcSet = [.. srcSet];
        HashSet<T> dupDstSet = [.. dstSet];
        HashSet<T> sharedSet = [.. srcSet];
        sharedSet.IntersectWith(dupDstSet);
        dupSrcSet.ExceptWith(sharedSet);
        dupDstSet.ExceptWith(sharedSet);
        return (dupSrcSet, dupDstSet, sharedSet);
    }
}
