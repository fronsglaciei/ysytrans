using FG.Defs.YSYard.Translations;
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

    private LanguagePathDefs _pathDef = null!;

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
        this._pathDef = new(pluginFolderPath);
        if (!this._pathDef.IsValid)
        {
            return;
        }
        this._pathDef.EnsureAllCreated();
    }

    public bool TryGetTranslation(KeyNotification kn, out string translation)
    {
        translation = string.Empty;
        var path = this._pathDef.GetTranslationFilePath(kn);
        if (File.Exists(path))
        {
            translation = File.ReadAllText(path, Encoding.UTF8);
            return true;
        }

        return false;
    }

    public void SaveTranslation(KeyNotification kn, string translation)
    {
        var path = this._pathDef.GetTranslationFilePath(kn);

        // converts new-line codes
        var tmp = translation.Replace("\r\n", "\n");
        tmp = tmp.Replace("\r", "\n");

        File.WriteAllText(path, tmp, Encoding.UTF8);
    }

    public async Task<IEnumerable<KeyNotification>> SearchTranslationAsync(string pattern)
    {
        if (string.IsNullOrEmpty(pattern) || !this._pathDef.IsValid)
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
        var taskLanguages = Directory.EnumerateFiles(this._pathDef.LanguagesPath).Select(x => Task.Run(() =>
        {
            if (this._cts.IsCancellationRequested)
            {
                return null;
            }
            if (this._pathDef.TryGetKeyFromPath(x, out var key))
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

        var taskLanguageTalks = Directory.EnumerateFiles(this._pathDef.LanguageTalksPath).Select(x => Task.Run(() =>
        {
            if (this._cts.IsCancellationRequested)
            {
                return null;
            }
            if (this._pathDef.TryGetKeyFromPath(x, out var key))
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
        var taskLanguages = Directory.EnumerateFiles(this._pathDef.LanguagesPath).Select(x => Task.Run(() =>
        {
            if (this._pathDef.TryGetKeyFromPath(x, out var key))
            {
                var text = File.ReadAllText(x, Encoding.UTF8);
                tmpLanguages.TryAdd(key, text);
            }
        }));
        var tmpLanguageTalks = new ConcurrentDictionary<int, string>();
        var taskLanguageTalks = Directory.EnumerateFiles(this._pathDef.LanguageTalksPath).Select(x => Task.Run(() =>
        {
            if (this._pathDef.TryGetKeyFromPath(x, out var key))
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
        if (!this._pathDef.IsValid)
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
            var path = this._pathDef.GetTranslationFilePath(new KeyNotification
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
            var path = this._pathDef.GetTranslationFilePath(new KeyNotification
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
        if (!this._pathDef.IsValid)
        {
            return false;
        }

        var tmpLanguages = new ConcurrentDictionary<int, string>();
        var taskLanguages = Directory.EnumerateFiles(this._pathDef.LanguagesPath).Select(x => Task.Run(() =>
        {
            if (this._pathDef.TryGetKeyFromPath(x, out var key))
            {
                var text = File.ReadAllText(x, Encoding.UTF8);
                tmpLanguages.TryAdd(key, text);
            }
        }));
        var tmpLanguageTalks = new ConcurrentDictionary<int, string>();
        var taskLanguageTalks = Directory.EnumerateFiles(this._pathDef.LanguageTalksPath).Select(x => Task.Run(() =>
        {
            if (this._pathDef.TryGetKeyFromPath(x, out var key))
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
        File.WriteAllText(this._pathDef.TranslatedLanguagesSerializedPath, json0, Encoding.UTF8);

        var tlt = new TranslatedLanguageTalks
        {
            Dictionary = tmpLanguageTalks.ToDictionary()
        };
        var json1 = JsonSerializer.Serialize(tlt, _jsonOpts);
        File.WriteAllText(this._pathDef.TranslatedLanguageTalksSerializedPath, json1, Encoding.UTF8);

        return true;
    }

    public void Dispose()
    {
        this._cts.Cancel();
        this._cts.Dispose();
        GC.SuppressFinalize(this);
    }
}
