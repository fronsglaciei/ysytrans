using FG.Defs.YSYard.Translations;
using FG.Defs.YSYard.Translations.Devs;
using FG.Utils.YSYard.Translations.Contracts.Models;
using FG.Utils.YSYard.Translations.Contracts.Services;
using FG.Utils.YSYard.Translations.Models;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Text.Unicode;

namespace FG.Utils.YSYard.Translations.Services;

public class TranslationRepositoryService : ITranslationRepositoryService, IDisposable
{
    private readonly IWritableOptions<AppConfig> _config;

    private DevelopmentPathDefs _pathDefDev = null!;

    private readonly CancellationTokenSource _cts = new();

    private static readonly JsonSerializerOptions _jsonOpts = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
    };

    public TranslationRepositoryService(IWritableOptions<AppConfig> config)
    {
        this._config = config;
        this.SetPluginFolderPath(this._config.Value.PluginFolderPath);
    }

    public void SetPluginFolderPath(string pluginFolderPath)
    {
        this._pathDefDev = new(pluginFolderPath);
        if (!this._pathDefDev.IsValid)
        {
            return;
        }
        this._pathDefDev.EnsureAllCreated();
    }

    public bool TryGetTranslation(KeyNotification kn, out string translation)
    {
        translation = string.Empty;
        var path = this._pathDefDev.GetTranslationFilePath(kn);
        if (File.Exists(path))
        {
            translation = File.ReadAllText(path, Encoding.UTF8);
            return true;
        }

        return false;
    }

    public void SaveTranslation(KeyNotification kn, string translation)
    {
        var path = this._pathDefDev.GetTranslationFilePath(kn);

        // converts new-line codes
        var tmp = translation.Replace("\r\n", "\n");
        tmp = tmp.Replace("\r", "\n");

        File.WriteAllText(path, tmp, Encoding.UTF8);
    }

    public async Task<IEnumerable<KeyNotification>> SearchTranslationAsync(string pattern)
    {
        if (string.IsNullOrEmpty(pattern) || !this._pathDefDev.IsValid)
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
        var taskLanguages = Directory.EnumerateFiles(this._pathDefDev.LanguagesPath).Select(x => Task.Run(() =>
        {
            if (this._cts.IsCancellationRequested)
            {
                return null;
            }
            if (this._pathDefDev.TryGetKeyFromPath(x, out var key))
            {
                var text = File.ReadAllText(x, Encoding.UTF8);
                if (re.IsMatch(text))
                {
                    return new KeyNotification
                    {
                        KeyType = LanguageKeyTypes.Language,
                        Key = key
                    };
                }
            }
            return null;
        }));
        var ret = new List<KeyNotification>();
        var tmpKns = await Task.WhenAll(taskLanguages).ConfigureAwait(false);
        if (this._cts.IsCancellationRequested)
        {
            return ret;
        }
        ret.AddRange(tmpKns.Where(x => x != null).OfType<KeyNotification>());

        var taskLanguageTalks = Directory.EnumerateFiles(this._pathDefDev.LanguageTalksPath).Select(x => Task.Run(() =>
        {
            if (this._cts.IsCancellationRequested)
            {
                return null;
            }
            if (this._pathDefDev.TryGetKeyFromPath(x, out var key))
            {
                var text = File.ReadAllText(x, Encoding.UTF8);
                if (re.IsMatch(text))
                {
                    return new KeyNotification
                    {
                        KeyType = LanguageKeyTypes.LanguageTalk,
                        Key = key
                    };
                }
            }
            return null;
        }));
        tmpKns = await Task.WhenAll(taskLanguageTalks).ConfigureAwait(false);
        if (this._cts.IsCancellationRequested)
        {
            return ret;
        }
        ret.AddRange(tmpKns.Where(x => x != null).OfType<KeyNotification>());
        return ret;
    }

    public async Task<bool> SaveBackupAsync(string path)
    {
        var tmpLanguages = new ConcurrentDictionary<int, string>();
        var taskLanguages = Directory.EnumerateFiles(this._pathDefDev.LanguagesPath).Select(x => Task.Run(() =>
        {
            if (this._pathDefDev.TryGetKeyFromPath(x, out var key))
            {
                var text = File.ReadAllText(x, Encoding.UTF8);
                tmpLanguages.TryAdd(key, text);
            }
        }));
        var tmpLanguageTalks = new ConcurrentDictionary<int, string>();
        var taskLanguageTalks = Directory.EnumerateFiles(this._pathDefDev.LanguageTalksPath).Select(x => Task.Run(() =>
        {
            if (this._pathDefDev.TryGetKeyFromPath(x, out var key))
            {
                var text = File.ReadAllText(x, Encoding.UTF8);
                tmpLanguageTalks.TryAdd(key, text);
            }
        }));
        await Task.WhenAll(new[] { taskLanguages, taskLanguageTalks }.SelectMany(x => x)).ConfigureAwait(false);

        var languages = tmpLanguages.ToList();
        languages.Sort((a, b) => a.Key.CompareTo(b.Key));
        var languageTalks = tmpLanguageTalks.ToList();
        languageTalks.Sort((a, b) => a.Key.CompareTo(b.Key));

        var json = JsonSerializer.Serialize(new TranslationsBackup
        {
            Languages = languages,
            LanguageTalks = languageTalks
        }, _jsonOpts);
        if (string.IsNullOrEmpty(json))
        {
            return false;
        }

        try
        {
            File.WriteAllText(path, json, Encoding.UTF8);
        }
        catch
        {
            return false;
        }

        return true;
    }

    public async Task<bool> LoadBackupAsync(string path)
    {
        if (!this._pathDefDev.IsValid)
        {
            return false;
        }

        var json = string.Empty;
        try
        {
            json = File.ReadAllText(path, Encoding.UTF8);
        }
        catch
        {
            return false;
        }

        var obj = JsonSerializer.Deserialize<TranslationsBackup>(json);
        if (obj == null)
        {
            return false;
        }

        var taskLanguages = obj.Languages.Select(x => Task.Run(() =>
        {
            if (this._cts.IsCancellationRequested)
            {
                return;
            }
            var path = this._pathDefDev.GetTranslationFilePath(new KeyNotification
            {
                KeyType = LanguageKeyTypes.Language,
                Key = x.Key,
            });
            File.WriteAllText(path, x.Value, Encoding.UTF8);
        }, this._cts.Token));
        var taskLanguageTalks = obj.LanguageTalks.Select(x => Task.Run(() =>
        {
            if (this._cts.IsCancellationRequested)
            {
                return;
            }
            var path = this._pathDefDev.GetTranslationFilePath(new KeyNotification
            {
                KeyType = LanguageKeyTypes.LanguageTalk,
                Key = x.Key,
            });
            File.WriteAllText(path, x.Value, Encoding.UTF8);
        }, this._cts.Token));
        await Task.WhenAll(new[] { taskLanguages, taskLanguageTalks }.SelectMany(x => x)).ConfigureAwait(false);
        return true;
    }

    public async Task<bool> SerializeAsync()
    {
        if (!this._pathDefDev.IsValid)
        {
            return false;
        }

        var tmpLanguages = new ConcurrentDictionary<int, string>();
        var taskLanguages = Directory.EnumerateFiles(this._pathDefDev.LanguagesPath).Select(x => Task.Run(() =>
        {
            if (this._pathDefDev.TryGetKeyFromPath(x, out var key))
            {
                var text = File.ReadAllText(x, Encoding.UTF8);
                tmpLanguages.TryAdd(key, text);
            }
        }));
        var tmpLanguageTalks = new ConcurrentDictionary<int, string>();
        var taskLanguageTalks = Directory.EnumerateFiles(this._pathDefDev.LanguageTalksPath).Select(x => Task.Run(() =>
        {
            if (this._pathDefDev.TryGetKeyFromPath(x, out var key))
            {
                var text = File.ReadAllText(x, Encoding.UTF8);
                tmpLanguageTalks.TryAdd(key, text);
            }
        }));
        await Task.WhenAll(new[] { taskLanguages, taskLanguageTalks }.SelectMany(x => x)).ConfigureAwait(false);

        var tl = new TranslatedLanguages
        {
            Dictionary = tmpLanguages.ToDictionary()
        };
        var json0 = JsonSerializer.Serialize(tl, _jsonOpts);
        var pathDefMod = new LanguagePathDefs(this._pathDefDev.PluginRootPath);
        File.WriteAllText(pathDefMod.TranslatedLanguagesSerializedPath, json0, Encoding.UTF8);

        var tlt = new TranslatedLanguageTalks
        {
            Dictionary = tmpLanguageTalks.ToDictionary()
        };
        var json1 = JsonSerializer.Serialize(tlt, _jsonOpts);
        File.WriteAllText(pathDefMod.TranslatedLanguageTalksSerializedPath, json1, Encoding.UTF8);

        return true;
    }

    public async Task ReportDiffAsync(string oldPluginFolderPath)
    {
        if (this._pathDefDev == null)
        {
            return;
        }
        if (string.IsNullOrEmpty(oldPluginFolderPath) || !Directory.Exists(oldPluginFolderPath))
        {
            return;
        }

        var oldPathDef = new DevelopmentPathDefs(oldPluginFolderPath);
        if (!oldPathDef.IsValid)
        {
            return;
        }

        var oldLanguages = new ConcurrentDictionary<int, string>();
        var taskOldLanguages = Directory.EnumerateFiles(oldPathDef.LanguagesPath).Select(x => Task.Run(() =>
        {
            if (oldPathDef.TryGetKeyFromPath(x, out var key))
            {
                var text = File.ReadAllText(x, Encoding.UTF8);
                oldLanguages.TryAdd(key, text);
            }
        }));
        var oldLanguageTalks = new ConcurrentDictionary<int, string>();
        var taskOldLanguageTalks = Directory.EnumerateFiles(oldPathDef.LanguageTalksPath).Select(x => Task.Run(() =>
        {
            if (oldPathDef.TryGetKeyFromPath(x, out var key))
            {
                var text = File.ReadAllText(x, Encoding.UTF8);
                oldLanguageTalks.TryAdd(key, text);
            }
        }));
        await Task.WhenAll(new[] { taskOldLanguages, taskOldLanguageTalks }.SelectMany(x => x)).ConfigureAwait(false);

        var curLanguages = new ConcurrentDictionary<int, string>();
        var taskCurLanguages = Directory.EnumerateFiles(this._pathDefDev.LanguagesPath).Select(x => Task.Run(() =>
        {
            if (this._pathDefDev.TryGetKeyFromPath(x, out var key))
            {
                var text = File.ReadAllText(x, Encoding.UTF8);
                curLanguages.TryAdd(key, text);
            }
        }));
        var curLanguageTalks = new ConcurrentDictionary<int, string>();
        var taskCurLanguageTalks = Directory.EnumerateFiles(this._pathDefDev.LanguageTalksPath).Select(x => Task.Run(() =>
        {
            if (this._pathDefDev.TryGetKeyFromPath(x, out var key))
            {
                var text = File.ReadAllText(x, Encoding.UTF8);
                curLanguageTalks.TryAdd(key, text);
            }
        }));
        await Task.WhenAll(new[] { taskCurLanguages, taskCurLanguageTalks }.SelectMany(x => x)).ConfigureAwait(false);

        var sb = new StringBuilder();
        var oldLanguageIndices = oldLanguages.Keys.ToHashSet();
        oldLanguageIndices.ExceptWith(curLanguages.Keys);
        sb.AppendLine("[Deprecated Languages]");
        foreach (var oldLanguageIndex in oldLanguageIndices)
        {
            sb.AppendLine($"  {oldLanguageIndex}");
        }
        sb.AppendLine();

        var curLanguageIndices = curLanguages.Keys.ToHashSet();
        curLanguageIndices.ExceptWith(oldLanguages.Keys);
        sb.AppendLine("[Appended Languages]");
        foreach (var curLanguageIndex in curLanguageIndices)
        {
            sb.AppendLine($"  {curLanguageIndex}");
        }
        sb.AppendLine();

        var sharedLanguageIndices = oldLanguages.Keys.ToHashSet();
        sharedLanguageIndices.IntersectWith(curLanguages.Keys);
        sb.AppendLine("[Updated Languages]");
        foreach (var sharedIndex in sharedLanguageIndices)
        {
            if (oldLanguages.TryGetValue(sharedIndex, out var oldText)
                && curLanguages.TryGetValue(sharedIndex, out var curText)
                && oldText != curText)
            {
                sb.AppendLine($"<{sharedIndex}>---");
                sb.AppendLine(oldText);
                sb.AppendLine("↓");
                sb.AppendLine(curText);
                sb.AppendLine("---");
            }
        }
        sb.AppendLine();

        var oldLanguageTalkIndices = oldLanguageTalks.Keys.ToHashSet();
        oldLanguageTalkIndices.ExceptWith(curLanguageTalks.Keys);
        sb.AppendLine("[Deprecated LanguageTalks]");
        foreach (var oldLanguageTalkIndex in oldLanguageTalkIndices)
        {
            sb.AppendLine($"  {oldLanguageTalkIndex}");
        }
        sb.AppendLine();

        var curLanguageTalkIndices = curLanguageTalks.Keys.ToHashSet();
        curLanguageTalkIndices.ExceptWith(oldLanguageTalks.Keys);
        sb.AppendLine("[Appended LanguageTalks]");
        foreach (var curLanguageTalkIndex in curLanguageTalkIndices)
        {
            sb.AppendLine($"  {curLanguageTalkIndex}");
        }
        sb.AppendLine();

        var sharedLanguageTalkIndices = oldLanguageTalks.Keys.ToHashSet();
        sharedLanguageTalkIndices.IntersectWith(curLanguageTalks.Keys);
        sb.AppendLine("[Updated LanguageTalks]");
        foreach (var sharedIndex in sharedLanguageTalkIndices)
        {
            if (oldLanguageTalks.TryGetValue(sharedIndex, out var oldText)
                && curLanguageTalks.TryGetValue(sharedIndex, out var curText)
                && oldText != curText)
            {
                sb.AppendLine($"<{sharedIndex}>---");
                sb.AppendLine(oldText);
                sb.AppendLine("↓");
                sb.AppendLine(curText);
                sb.AppendLine("---");
            }
        }
        sb.AppendLine();

        var reportPath = Path.Combine(this._pathDefDev.PluginRootPath, "diff_Translations.txt");
        File.WriteAllText(reportPath, sb.ToString(), Encoding.UTF8);
    }

    public void Dispose()
    {
        this._cts.Cancel();
        this._cts.Dispose();
        GC.SuppressFinalize(this);
    }
}
