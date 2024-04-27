using FG.Defs.YSYard.Translations;
using FG.Utils.YSYard.Translations.Contracts.Models;
using FG.Utils.YSYard.Translations.Contracts.Services;
using FG.Utils.YSYard.Translations.Helpers;
using FG.Utils.YSYard.Translations.Models;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace FG.Utils.YSYard.Translations.Services;

public class LanguageRepositoryService : ILanguageRepositoryService, IDisposable
{
	private readonly IWritableOptions<AppConfig> _config;

	private readonly ReaderWriterLockSlim _lock = new();

	private LanguagePathDefs _pathDef = null!;

	private readonly List<int> _languageIndices = [];

	private readonly Dictionary<int, string> _languageRefs = [];

	private readonly List<int> _languageTalkIndices = [];

	private readonly Dictionary<int, string> _languageTalkRefs = [];

	private readonly Dictionary<KeyNotification, LanguageContainer> _containerCache = [];

	private bool _isFullyLoaded = false;

	private readonly CancellationTokenSource _cts = new();

	public LanguageRepositoryService(IWritableOptions<AppConfig> config)
	{
		this._config = config;
		this.SetPluginFolderPath(this._config.Value.PluginFolderPath);
	}

	public void SetPluginFolderPath(string pluginFolderPath)
	{
		this._languageIndices.Clear();
		this._languageRefs.Clear();
		this._languageTalkIndices.Clear();
		this._languageTalkRefs.Clear();

		this._pathDef = new(pluginFolderPath);
		if (!this._pathDef.IsValid)
		{
			return;
		}
		this._pathDef.EnsureAllCreated();

		foreach (var file in Directory.EnumerateFiles(this._pathDef.LanguagesExportedPath))
		{
			if (this._pathDef.TryGetKeyFromPath(file, out var key))
			{
				this._languageRefs[key] = file;
			}
		}
		this._languageIndices.AddRange(this._languageRefs.Keys);
		this._languageIndices.Sort();

		foreach (var file in Directory.EnumerateFiles(this._pathDef.LanguageTalksExportedPath))
		{
			if (this._pathDef.TryGetKeyFromPath(file, out var key))
			{
				this._languageTalkRefs[key] = file;
			}
		}
		this._languageTalkIndices.AddRange(this._languageTalkRefs.Keys);
		this._languageTalkIndices.Sort();
	}

    public bool TryGetContainer(KeyNotification kn, [MaybeNullWhen(false)] out LanguageContainer container)
    {
		container = null;
		if (this._containerCache.TryGetValue(kn, out var tmpContainer))
		{
			container = tmpContainer;
			return true;
		}

        if (kn.KeyType == LanguageKeyTypes.Language)
		{
			if (this._languageRefs.TryGetValue(kn.Key, out var path))
			{
				return this.TryLoadLanguageContainer(path, kn.KeyType, out container);
			}
		}
		else if (kn.KeyType == LanguageKeyTypes.LanguageTalk)
		{
			if (this._languageTalkRefs.TryGetValue(kn.Key, out var path))
			{
				return this.TryLoadLanguageContainer(path, kn.KeyType, out container);
			}
		}
		return false;
    }

	private bool TryLoadLanguageContainer(string path, LanguageKeyTypes keyType, [MaybeNullWhen(false)] out LanguageContainer lc)
	{
		lc = null;
		var json = File.ReadAllText(path, Encoding.UTF8);
		if (string.IsNullOrEmpty(json))
		{
			return false;
		}
		if (keyType == LanguageKeyTypes.Language)
		{
			var lang = JsonSerializer.Deserialize<LanguageExport>(json);
			if (lang == null)
			{
				return false;
			}
			lc = new LanguageContainer
			{
				KeyNotification = new KeyNotification
				{
					KeyType = keyType,
					Key = lang.Key
				},
				SimpleChinese = lang.SimpleChinese,
				TraditionalChinese = lang.TraditionalChinese,
				English = lang.English,
				Japanese = lang.Japanese
			};
			using var wl = new WriteLock(this._lock);
			this._containerCache[lc.KeyNotification] = lc;
			return true;
		}
		else if (keyType == LanguageKeyTypes.LanguageTalk)
		{
            var langTalk = JsonSerializer.Deserialize<LanguageTalkExport>(json);
            if (langTalk == null)
            {
                return false;
            }
            lc = new LanguageContainer
            {
                KeyNotification = new KeyNotification
				{
					KeyType = keyType,
					Key = langTalk.Key
				},
                SimpleChinese = langTalk.SimpleChinese,
                TraditionalChinese = langTalk.TraditionalChinese,
                English = langTalk.English,
                Japanese = langTalk.Japanese
            };
			using var wl = new WriteLock(this._lock);
			this._containerCache[lc.KeyNotification] = lc;
			return true;
        }
		return false;
	}

    public bool TryGetNextContainer(KeyNotification kn, int indexOffset, [MaybeNullWhen(false)] out LanguageContainer container)
    {
		container = null;
		if (kn.KeyType == LanguageKeyTypes.Language)
		{
			var idx = this._languageIndices.IndexOf(kn.Key);
			if (idx < 0)
			{
				return false;
			}
			var nextIdx = idx + indexOffset;
			if (nextIdx < 0)
			{
				return this.TryGetContainer(new KeyNotification
				{
					KeyType = kn.KeyType,
					Key = this._languageIndices.First()
				}, out container);
			}
			else if (this._languageIndices.Count <= nextIdx)
			{
                return this.TryGetContainer(new KeyNotification
                {
                    KeyType = kn.KeyType,
                    Key = this._languageIndices.Last()
                }, out container);
            }
			else
			{
				return this.TryGetContainer(new KeyNotification
				{
					KeyType = kn.KeyType,
					Key = this._languageIndices[nextIdx]
				}, out container);
            }
		}
		else if (kn.KeyType == LanguageKeyTypes.LanguageTalk)
		{
            var idx = this._languageTalkIndices.IndexOf(kn.Key);
            if (idx < 0)
            {
                return false;
            }
            var nextIdx = idx + indexOffset;
            if (nextIdx < 0)
            {
                return this.TryGetContainer(new KeyNotification
                {
                    KeyType = kn.KeyType,
                    Key = this._languageTalkIndices.First()
                }, out container);
            }
            else if (this._languageTalkIndices.Count <= nextIdx)
            {
                return this.TryGetContainer(new KeyNotification
                {
                    KeyType = kn.KeyType,
                    Key = this._languageTalkIndices.Last()
                }, out container);
            }
            else
            {
                return this.TryGetContainer(new KeyNotification
                {
                    KeyType = kn.KeyType,
                    Key = this._languageTalkIndices[nextIdx]
                }, out container);
            }
        }
		return false;
    }

	public async Task<IEnumerable<KeyNotification>> SearchContainersAsync(string pattern)
	{
		if (string.IsNullOrEmpty(pattern))
		{
            return [];
        }

		await this.LoadAllLanguagesAsync().ConfigureAwait(false);

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
		var tasks = this._containerCache.Values.Select(x => Task.Run(() =>
		{
			if (this._cts.IsCancellationRequested)
			{
				return null;
			}
			return re.IsMatch(x.SimpleChinese ?? string.Empty) ? x : null;
        }));
		var tmpLCs = await Task.WhenAll(tasks).ConfigureAwait(false);
		if (this._cts.IsCancellationRequested)
		{
            return [];
        }
		return tmpLCs.Where(x => x != null).OfType<LanguageContainer>().Select(x => x.KeyNotification);
	}

	private async Task LoadAllLanguagesAsync()
	{
		if (this._isFullyLoaded)
		{
			return;
		}

		var taskLanguages = this._languageRefs.Select(x => Task.Run(() =>
		{
			if (this._cts.IsCancellationRequested)
			{
				return;
			}
            this.TryLoadLanguageContainer(x.Value, LanguageKeyTypes.Language, out var lc);
        }));
		await Task.WhenAll(taskLanguages).ConfigureAwait(false);

		if (this._cts.IsCancellationRequested)
		{
			return;
		}

		var taskLanguageTalks = this._languageTalkRefs.Select(x => Task.Run(() =>
		{
            if (this._cts.IsCancellationRequested)
            {
                return;
            }
            this.TryLoadLanguageContainer(x.Value, LanguageKeyTypes.LanguageTalk, out var lc);
        }));
        await Task.WhenAll(taskLanguageTalks).ConfigureAwait(false);

		using var wl = new WriteLock(this._lock);
		this._isFullyLoaded = true;
	}

	public void Dispose()
	{
		this._cts.Cancel();
		this._cts.Dispose();
		this._lock.Dispose();
		GC.SuppressFinalize(this);
	}
}
