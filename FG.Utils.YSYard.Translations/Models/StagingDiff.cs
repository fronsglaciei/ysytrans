using FG.Defs.YSYard.Translations.Devs;
using System.Text;

namespace FG.Utils.YSYard.Translations.Models;

public class StagingDiff
{
    public Dictionary<LanguageKeyTypes, List<int>> KeyAdditions
    {
        get; set;
    } = [];

    public Dictionary<LanguageKeyTypes, List<int>> KeyRemovals
    {
        get; set;
    } = [];

    public Dictionary<LanguageKeyTypes, Dictionary<int, StagingStringUpdate>> StringUpdates
    {
        get; set;
    } = [];

    public Dictionary<LanguageKeyTypes, TranslationStatistics> IndividualConverages
    {
        get; set;
    } = [];

    public TranslationStatistics TotalCoverage
    {
        get; set;
    } = new();

    public string ToReportString()
    {
        var sb = new StringBuilder();
        sb.AppendLine("# StagingDiff report");
        sb.AppendLine();

        if (0 < this.KeyAdditions.Count)
        {
            sb.AppendLine("## Added keys");
            sb.AppendLine();
            foreach (var kvp in this.KeyAdditions)
            {
                sb.AppendLine($"* {kvp.Key}");
                var tmp = string.Join(", ", kvp.Value);
                sb.AppendLine($"  * {tmp}");
            }
            sb.AppendLine();
        }

        if (0 < this.KeyRemovals.Count)
        {
            sb.AppendLine("## Removed keys");
            sb.AppendLine();
            foreach (var kvp in this.KeyRemovals)
            {
                sb.AppendLine($"* {kvp.Key}");
                var tmp = string.Join(", ", kvp.Value);
                sb.AppendLine($"  * {tmp}");
            }
            sb.AppendLine();
        }

        if (0 < this.StringUpdates.Count)
        {
            sb.AppendLine("## String updates");
            sb.AppendLine();
            foreach (var kvpAsset in this.StringUpdates)
            {
                sb.AppendLine($"* {kvpAsset.Key}");
                foreach (var kvpKey in kvpAsset.Value)
                {
                    sb.AppendLine($"  * {kvpKey.Key}");
                    if (kvpKey.Value.Original != null)
                    {
                        sb.AppendLine($"    * Original");
                        var pair = kvpKey.Value.Original;
                        var strBefore =
                            string.IsNullOrEmpty(pair.Before)
                            ? "(empty)" : pair.Before;
                        sb.AppendLine($"      * {strBefore}");
                        var strAfter =
                            string.IsNullOrEmpty(pair.After)
                            ? "(empty)" : pair.After;
                        sb.AppendLine($"      * {strAfter}");
                    }
                    if (kvpKey.Value.Translation != null)
                    {
                        sb.AppendLine($"    * Translation");
                        var pair = kvpKey.Value.Translation;
                        var strBefore =
                            string.IsNullOrEmpty(pair.Before)
                            ? "(empty)" : pair.Before;
                        sb.AppendLine($"      * {strBefore}");
                        var strAfter =
                            string.IsNullOrEmpty(pair.After)
                            ? "(empty)" : pair.After;
                        sb.AppendLine($"      * {strAfter}");
                    }
                }
            }
            sb.AppendLine();
        }

        if (0 < this.IndividualConverages.Count)
        {
            sb.AppendLine("## Translation coverages");
            sb.AppendLine();
            sb.AppendLine("|種別|比較元| |比較先|");
            sb.AppendLine("|:---:|:---:|:---:|:---:|");
            foreach (var kvp in this.IndividualConverages)
            {
                var tmpTs = kvp.Value;
                var beforeRate =
                    tmpTs.BeforeTotal < 1
                    ? 0.0
                    : tmpTs.BeforeTranslated / (double)tmpTs.BeforeTotal * 100.0;
                var strBefore = $"{tmpTs.BeforeTranslated}/{tmpTs.BeforeTotal} ({beforeRate:F2}%)";
                var afterRate =
                    tmpTs.AfterTotal < 1
                    ? 0.0
                    : tmpTs.AfterTranslated / (double)tmpTs.AfterTotal * 100.0;
                var strAfter = $"{tmpTs.AfterTranslated}/{tmpTs.AfterTotal} ({afterRate:F2}%)";
                sb.AppendLine($"|{kvp.Key}|{strBefore}|→|{strAfter}|");
            }
            if (0 < this.TotalCoverage.BeforeTotal
                || 0 < this.TotalCoverage.AfterTotal)
            {
                sb.AppendLine("| | | | |");

                var tmpTs = this.TotalCoverage;
                var beforeRate =
                    tmpTs.BeforeTotal < 1
                    ? 0.0
                    : tmpTs.BeforeTranslated / (double)tmpTs.BeforeTotal * 100.0;
                var strBefore = $"{tmpTs.BeforeTranslated}/{tmpTs.BeforeTotal} ({beforeRate:F2}%)";
                var afterRate =
                    tmpTs.AfterTotal < 1
                    ? 0.0
                    : tmpTs.AfterTranslated / (double)tmpTs.AfterTotal * 100.0;
                var strAfter = $"{tmpTs.AfterTranslated}/{tmpTs.AfterTotal} ({afterRate:F2}%)";
                sb.AppendLine($"|total|{strBefore}|→|{strAfter}|");
            }
        }

        return sb.ToString();
    }
}
