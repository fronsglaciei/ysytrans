using FG.Defs.YSYard.Translations;
using FG.Defs.YSYard.Translations.Devs;
using FG.Utils.YSYard.Translations.Contracts.Models;
using FG.Utils.YSYard.Translations.Enums;
using FG.Utils.YSYard.Translations.Helpers;
using FG.Utils.YSYard.Translations.Models;
using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using System.Security;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml;

namespace FG.Utils.YSYard.Translations.Services;

public class TranslationStoreService : IDisposable
{
    private const string TEMPLATE_DOCX = "Assets/template.docx";

    private readonly IWritableOptions<AppConfig> _appConfig;

    private LanguagePathDefs _langPathDef = null!;

    private DevelopmentPathDefs _devPathDef = null!;

    private StagingLanguage _stagingCache = new();

    private readonly Dictionary<LanguageKeyTypes, List<int>> _indexToKeyMap = [];

    private readonly Dictionary<LanguageKeyTypes, Dictionary<int, int>> _keyToIndexMap = [];

    private bool _temporarySaveRequested = false;

    private readonly Task _temporarySaveTask;

    private readonly ReaderWriterLockSlim _lock = new();

    private readonly CancellationTokenSource _cts = new();

    public TranslationStoreService(
        IWritableOptions<AppConfig> appConfig)
    {
        this._appConfig = appConfig;
        this.SetOutputDir(this._appConfig.Value.PluginMainDirPath);
        this.SetStagingDir(this._appConfig.Value.PluginDevDirPath);
        this._temporarySaveTask = Task.Run(this.TemporarySaveWorkerAsync);
    }

    public void SetOutputDir(string outputDir)
    {
        this._langPathDef = new LanguagePathDefs(outputDir);
        if (!this._langPathDef.IsValid)
        {
            return;
        }
    }

    public void SetStagingDir(string stagingDir)
    {
        this._devPathDef = new DevelopmentPathDefs(stagingDir);
        if (!this._devPathDef.IsValid)
        {
            return;
        }
        this.LoadCache();
    }

    public bool TryGetContainer(
        LanguageKeyTypes keyType, int key, [MaybeNullWhen(false)] out StagingLanguageContainer container)
    {
        container = null;
        return keyType switch
        {
            LanguageKeyTypes.Language => this._stagingCache.Languages.TryGetValue(key, out container),
            LanguageKeyTypes.LanguageTalk => this._stagingCache.LanguageTalks.TryGetValue(key, out container),
            _ => false,
        };
    }

    public bool TryGetContainer(LanguageKey kn, [MaybeNullWhen(false)] out StagingLanguageContainer container)
        => this.TryGetContainer(kn.KeyType, kn.Key, out container);

    public bool TryGetTalkContainer(
        int languageTalkKey,
        [MaybeNullWhen(false)] out (StagingLanguageContainer container, LanguageKey kn) tup)
    {
        tup = default;
        var nextKn = new LanguageKey
        {
            KeyType = LanguageKeyTypes.LanguageTalk,
            Key = languageTalkKey,
            TimeStamp = DateTime.Now
        };
        if (!this.TryGetContainer(nextKn, out var nextContainer))
        {
            return false;
        }
        tup = (nextContainer, nextKn);
        return true;
    }

    public bool TryGetKeyTypeFirstContainer(
        LanguageKeyTypes keyType,
        [MaybeNullWhen(false)] out (StagingLanguageContainer container, LanguageKey kn) tup)
    {
        tup = default;
        if (!this._indexToKeyMap.TryGetValue(keyType, out var listKeys) || listKeys.Count < 1)
        {
            return false;
        }
        if (!this.TryGetContainer(keyType, listKeys[0], out var container))
        {
            return false;
        }

        tup = (container, new LanguageKey
        {
            KeyType = keyType,
            Key = listKeys[0],
            TimeStamp = DateTime.Now
        });
        return true;
    }

    public bool TryGetNextContainer(
        LanguageKey kn, int offset,
        [MaybeNullWhen(false)] out (StagingLanguageContainer container, LanguageKey nextKn) tup)
    {
        tup = default;
        if (!this._keyToIndexMap.TryGetValue(kn.KeyType, out var dictAsset))
        {
            return false;
        }
        if (!dictAsset.TryGetValue(kn.Key, out var curIdx))
        {
            return false;
        }
        if (!this._indexToKeyMap.TryGetValue(kn.KeyType, out var listKeys) || listKeys.Count < 1)
        {
            return false;
        }

        var nextIdx = Math.Min(Math.Max(0, curIdx + offset), listKeys.Count - 1);
        var nextKey = listKeys[nextIdx];
        var nextKn = new LanguageKey
        {
            KeyType = kn.KeyType,
            Key = nextKey,
            TimeStamp = DateTime.Now
        };
        if (!this.TryGetContainer(nextKn, out var tmpContainer))
        {
            return false;
        }
        tup = (tmpContainer, nextKn);

        return true;
    }

    public async Task<List<LanguageKey>> SearchTextAsync(SearchRanges range, string pattern)
    {
        if (string.IsNullOrEmpty(pattern))
        {
            return [];
        }
        if (this._cts.IsCancellationRequested)
        {
            return [];
        }

        Regex re = null!;
        try
        {
            re = new Regex(pattern);
        }
        catch
        {
            return [];
        }

        var allDict = new Dictionary<LanguageKeyTypes, Dictionary<int, StagingLanguageContainer>>
        {
            [LanguageKeyTypes.Language] = this._stagingCache.Languages,
            [LanguageKeyTypes.LanguageTalk] = this._stagingCache.LanguageTalks
        };
        Func<KeyValuePair<int, StagingLanguageContainer>, string> accessor = range switch
        {
            SearchRanges.Original => x => x.Value.Original,
            SearchRanges.Translation => x => x.Value.Japanese,
            SearchRanges.Api => x => x.Value.ApiTranslated,
            _ => throw new NotImplementedException(),
        };
        var tasks = allDict.Select(x => x.Value.Select(y => Task.Run(() =>
        {
            return re.IsMatch(accessor.Invoke(y))
                ? new LanguageKey
                {
                    KeyType = x.Key,
                    Key = y.Key,
                    TimeStamp = DateTime.Now,
                }
                : null;
        }))).SelectMany(x => x);
        var tmpSlcs = await Task.WhenAll(tasks).ConfigureAwait(false);
        return [.. tmpSlcs.Where(x => x != null).OfType<LanguageKey>()];
    }

    public void ExportDocx(string destPath)
    {
        if (string.IsNullOrEmpty(destPath))
        {
            return;
        }
        if (!File.Exists(TEMPLATE_DOCX))
        {
            throw new FileNotFoundException(TEMPLATE_DOCX);
        }

        var sb = new StringBuilder();
        sb.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?><w:document xmlns:w=\"http://schemas.openxmlformats.org/wordprocessingml/2006/main\"><w:body>");
        var dictPairs = new[]
        {
            (LanguageKeyTypes.Language.ToString().ToLower(), this._stagingCache.Languages),
            (LanguageKeyTypes.LanguageTalk.ToString().ToLower(), this._stagingCache.LanguageTalks)
        };
        foreach (var (tmpType, tmpDict) in dictPairs)
        {
            foreach (var kvp in tmpDict)
            {
                var orig = kvp.Value.Original;
                if (string.IsNullOrWhiteSpace(orig))
                {
                    continue;
                }
                sb.Append($"<w:p tltp=\"{tmpType}\" tlid=\"{kvp.Key}\"><w:r><w:t>{SecurityElement.Escape(orig)}</w:t></w:r></w:p>");
            }
        }
        sb.Append("</w:body></w:document>");

        using var memStream = new MemoryStream();
        using (var tmplStream = new FileStream(TEMPLATE_DOCX, FileMode.Open))
        {
            tmplStream.CopyTo(memStream);
        }
        memStream.Seek(0, SeekOrigin.Begin);
        using (var zarch = new ZipArchive(memStream, ZipArchiveMode.Update, true))
        {
            zarch.GetEntry("word/document.xml")?.Delete();
            var entry = zarch.CreateEntry("word/document.xml");

            using var eStream = entry.Open();
            using var sw = new StreamWriter(eStream);
            sw.Write(sb.ToString());
        }
        using var fStream = new FileStream(destPath, FileMode.Create);
        memStream.Seek(0, SeekOrigin.Begin);
        memStream.CopyTo(fStream);
    }

    public void ImportDocx(string srcPath)
    {
        if (string.IsNullOrEmpty(srcPath))
        {
            return;
        }
        if (!File.Exists(srcPath))
        {
            return;
        }

        var xmlStr = "";
        using (var zarch = ZipFile.OpenRead(srcPath))
        {
            var entry = zarch.GetEntry("word/document.xml");
            if (entry == null)
            {
                return;
            }

            using var eStream = entry.Open();
            using var sr = new StreamReader(eStream);
            xmlStr = sr.ReadToEnd();
        }

        var xml = new XmlDocument();
        xml.LoadXml(xmlStr);
        var nsmgr = new XmlNamespaceManager(xml.NameTable);
        nsmgr.AddNamespace("w", "http://schemas.openxmlformats.org/wordprocessingml/2006/main");
        var nodes = xml.SelectNodes("//w:p", nsmgr);
        if (nodes == null)
        {
            return;
        }
        var allDicts = new Dictionary<string, Dictionary<int, StagingLanguageContainer>>
        {
            [LanguageKeyTypes.Language.ToString().ToLower()] = this._stagingCache.Languages,
            [LanguageKeyTypes.LanguageTalk.ToString().ToLower()] = this._stagingCache.LanguageTalks,
        };
        foreach (var node in nodes)
        {
            if (node is not XmlElement elem)
            {
                continue;
            }
            if (!int.TryParse(elem.GetAttribute("tlid"), out var tlid))
            {
                continue;
            }

            var tltp = elem.GetAttribute("tltp");
            if (allDicts.TryGetValue(tltp, out var tmpDict)
                && tmpDict.TryGetValue(tlid, out var slc))
            {
                slc.ApiTranslated = elem.InnerText;
            }
        }
        this.RequestTemporarySave();
    }

    public void SerializeTranslations(bool setPlaceholder)
    {
        var tmp = new TlData
        {
            Languages = [],
            LanguageTalks = [],
        };
        var dictPairs = new[]
        {
            (this._stagingCache.Languages, tmp.Languages),
            (this._stagingCache.LanguageTalks, tmp.LanguageTalks),
        };
        foreach (var (srcDict, dstDict) in dictPairs)
        {
            foreach (var kvp in srcDict)
            {
                var key = kvp.Key;
                var slc = kvp.Value;
                if (setPlaceholder)
                {
                    if (!string.IsNullOrWhiteSpace(slc.Japanese))
                    {
                        dstDict[key] = slc.Japanese;
                    }
                    else if (!string.IsNullOrWhiteSpace(slc.ApiTranslated))
                    {
                        dstDict[key] = slc.ApiTranslated;
                    }
                    else
                    {
                        dstDict[key] = slc.Placeholder;
                    }
                }
                else if (slc.IsTranslated)
                {
                    dstDict[key] = slc.Japanese;
                }
            }
        }
        var json = JsonSerializer.Serialize(tmp);
        File.WriteAllText(this._langPathDef.TlDataSerializedPath, json);
    }

    public bool ReportStagingDiff(
        string srcStagingPath, string dstReportPath, bool fromSrcToCache)
    {
        var json = File.ReadAllText(srcStagingPath);
        var srcSL = JsonSerializer.Deserialize<StagingLanguage>(json);
        if (srcSL == null)
        {
            return false;
        }

        var diff = this.DiffStagings(srcSL, fromSrcToCache);
        File.WriteAllText(dstReportPath, diff.ToReportString());

        return true;
    }

    public bool UpdateCacheWithStagingFile(string srcPath)
    {
        var json = File.ReadAllText(srcPath);
        var sl = JsonSerializer.Deserialize<StagingLanguage>(json);
        if (sl == null)
        {
            return false;
        }

        var dictPairs = new[]
        {
            (this._stagingCache.Languages, sl.Languages),
            (this._stagingCache.LanguageTalks, sl.LanguageTalks)
        };
        foreach (var (srcDict, dstDict) in dictPairs)
        {
            var tmpKeys = dstDict.Keys;
            foreach (var key in tmpKeys)
            {
                if (srcDict.TryGetValue(key, out var val))
                {
                    dstDict[key].Japanese = val.Japanese;
                    dstDict[key].IsTranslated = val.IsTranslated;
                }
            }
        }
        this._stagingCache = sl;
        this.SaveCache();
        return true;
    }

    public void RequestTemporarySave()
    {
        using var _wl = new WriteLock(this._lock);
        this._temporarySaveRequested = true;
    }

    private void LoadCache()
    {
        var pathToLoad = string.Empty;
        if (File.Exists(this._devPathDef.StagingCachePath))
        {
            pathToLoad = this._devPathDef.StagingCachePath;
        }
        else if (File.Exists(this._devPathDef.StagingJsonPath))
        {
            pathToLoad = this._devPathDef.StagingJsonPath;
        }
        if (string.IsNullOrEmpty(pathToLoad))
        {
            return;
        }

        var json = File.ReadAllText(pathToLoad);
        var tmpCache = JsonSerializer.Deserialize<StagingLanguage>(json);
        if (tmpCache == null)
        {
            return;
        }
        this._stagingCache = tmpCache;

        this._indexToKeyMap.Clear();
        this._keyToIndexMap.Clear();

        var dictPairs = new[]
        {
            (LanguageKeyTypes.Language, this._stagingCache.Languages),
            (LanguageKeyTypes.LanguageTalk, this._stagingCache.LanguageTalks)
        };
        foreach (var (tmpType, tmpDict) in dictPairs)
        {
            var tmpKeys = tmpDict.Keys.ToList();
            tmpKeys.Sort();
            this._indexToKeyMap[tmpType] = tmpKeys;
            var dst = this._keyToIndexMap.GetOrCreateGet(tmpType);
            for (var i = 0; i < tmpKeys.Count; i++)
            {
                dst[tmpKeys[i]] = i;
            }
        }
    }

    private async Task TemporarySaveWorkerAsync()
    {
        while (true)
        {
            if (this._cts.IsCancellationRequested)
            {
                return;
            }
            if (this._temporarySaveRequested)
            {
                try
                {
                    this.SaveCache();
                }
                catch { }
            }
            await Task.Delay(
                this._appConfig.Value.TemporarySaveInterval, this._cts.Token)
                .ConfigureAwait(false);
        }
    }

    private void SaveCache()
    {
        if (!this._devPathDef.IsValid)
        {
            return;
        }

        var json = JsonSerializer.Serialize(this._stagingCache);
        File.WriteAllText(this._devPathDef.StagingCachePath, json);
        if (this._temporarySaveRequested)
        {
            using var wl = new WriteLock(this._lock);
            this._temporarySaveRequested = false;
        }
    }

    private StagingDiff DiffStagings(
        StagingLanguage objSL, bool fromSrcToCache)
    {
        var from = fromSrcToCache ? objSL : this._stagingCache;
        var to = fromSrcToCache ? this._stagingCache : objSL;

        var keyAdditions = new Dictionary<LanguageKeyTypes, List<int>>();
        var keyRemovals = new Dictionary<LanguageKeyTypes, List<int>>();
        var stringUpdates = new Dictionary<LanguageKeyTypes, Dictionary<int, StagingStringUpdate>>();
        var sharedTypes = new[]
        {
            (LanguageKeyTypes.Language, from.Languages, to.Languages),
            (LanguageKeyTypes.LanguageTalk, from.LanguageTalks, to.LanguageTalks),
        };
        foreach (var (tmpType, fromDict, toDict) in sharedTypes)
        {
            var fromKeys = fromDict.Keys.ToHashSet();
            var toKeys = toDict.Keys.ToHashSet();
            var (onlyInFromKeys, onlyInToKeys, sharedKeys) =
                fromKeys.DiffWith(toKeys);

            if (0 < onlyInToKeys.Count)
            {
                keyAdditions[tmpType] = [.. onlyInToKeys];
            }
            if (0 < onlyInFromKeys.Count)
            {
                keyRemovals[tmpType] = [.. onlyInFromKeys];
            }

            foreach (var key in sharedKeys)
            {
                var fromOrig = fromDict[key].Original;
                var toOrig = toDict[key].Original;
                if (fromOrig != toOrig)
                {
                    var curUpdate = stringUpdates
                        .GetOrCreateGet(tmpType)
                        .GetOrCreateGet(key);
                    curUpdate.Original = new()
                    {
                        Before = fromOrig,
                        After = toOrig,
                    };
                }

                var fromJpn = fromDict[key].Japanese;
                var toJpn = toDict[key].Japanese;
                if (fromJpn != toJpn)
                {
                    var curUpdate = stringUpdates
                        .GetOrCreateGet(tmpType)
                        .GetOrCreateGet(key);
                    curUpdate.Translation = new()
                    {
                        Before = fromJpn,
                        After = toJpn,
                    };
                }
            }
        }

        var coverages = new Dictionary<LanguageKeyTypes, TranslationStatistics>();
        var fromDicts = new[]
        {
            (LanguageKeyTypes.Language, from.Languages),
            (LanguageKeyTypes.LanguageTalk, from.LanguageTalks),
        };
        foreach (var (tmpType, tmpDict) in fromDicts)
        {
            var ts = coverages.GetOrCreateGet(tmpType);
            foreach (var container in tmpDict.Values)
            {
                if (string.IsNullOrWhiteSpace(container.Original))
                {
                    continue;
                }
                ts.BeforeTotal++;
                if (!string.IsNullOrWhiteSpace(container.Japanese)
                    && container.Original != container.Japanese)
                {
                    ts.BeforeTranslated++;
                }
            }
        }
        var toDicts = new[]
        {
            (LanguageKeyTypes.Language, to.Languages),
            (LanguageKeyTypes.LanguageTalk, to.LanguageTalks),
        };
        foreach (var (tmpType, tmpDict) in toDicts)
        {
            var ts = coverages.GetOrCreateGet(tmpType);
            foreach (var container in tmpDict.Values)
            {
                if (string.IsNullOrWhiteSpace(container.Original))
                {
                    continue;
                }
                ts.AfterTotal++;
                if (!string.IsNullOrWhiteSpace(container.Japanese)
                    && container.Original != container.Japanese)
                {
                    ts.AfterTranslated++;
                }
            }
        }
        var totalCoverage = new TranslationStatistics();
        foreach (var ts in coverages.Values)
        {
            totalCoverage.BeforeTotal += ts.BeforeTotal;
            totalCoverage.BeforeTranslated += ts.BeforeTranslated;
            totalCoverage.AfterTotal += ts.AfterTotal;
            totalCoverage.AfterTranslated += ts.AfterTranslated;
        }

        return new()
        {
            KeyAdditions = keyAdditions,
            KeyRemovals = keyRemovals,
            StringUpdates = stringUpdates,
            IndividualConverages = coverages,
            TotalCoverage = totalCoverage
        };
    }

    public void Dispose()
    {
        this._cts.Cancel();
        this._cts?.Dispose();
        this.SaveCache();
        GC.SuppressFinalize(this);
    }
}
