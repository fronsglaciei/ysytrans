using FG.Defs.YSYard.Translations.Devs;
using FG.Utils.YSYard.Translations.Contracts.Models;
using FG.Utils.YSYard.Translations.Contracts.Services;
using FG.Utils.YSYard.Translations.Helpers;
using FG.Utils.YSYard.Translations.Models;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace FG.Utils.YSYard.Translations.Services;

public class LanguageRepositoryService : ILanguageRepositoryService, IDisposable
{
	private readonly IWritableOptions<AppConfig> _config;

	private readonly ReaderWriterLockSlim _lock = new();

	private DevelopmentPathDefs _pathDef = null!;

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

    public async Task ReportDiffAsync(string oldPluginFolderPath)
    {
        if (this._pathDef == null)
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

        if (!this._isFullyLoaded)
        {
            await this.LoadAllLanguagesAsync().ConfigureAwait(false);
        }

        var oldLanguages = new ConcurrentDictionary<int, string>();
        var taskOldLanguages = Directory.EnumerateFiles(oldPathDef.LanguagesExportedPath).Select(x => Task.Run(() =>
        {
            if (oldPathDef.TryGetKeyFromPath(x, out var key))
            {
                var json = File.ReadAllText(x, Encoding.UTF8);
                var lang = JsonSerializer.Deserialize<LanguageExport>(json);
                if (lang != null)
                {
                    oldLanguages[key] = lang.SimpleChinese;
                }
            }
        }));
        var oldLanguageTalks = new ConcurrentDictionary<int, string>();
        var taskOldLanguageTalks = Directory.EnumerateFiles(oldPathDef.LanguageTalksExportedPath).Select(x => Task.Run(() =>
        {
            if (oldPathDef.TryGetKeyFromPath(x, out var key))
            {
                var json = File.ReadAllText(x, Encoding.UTF8);
                var langTalk = JsonSerializer.Deserialize<LanguageTalkExport>(json);
                if (langTalk != null)
                {
                    oldLanguageTalks[key] = langTalk.SimpleChinese;
                }
            }
        }));
        await Task.WhenAll(new[] { taskOldLanguages, taskOldLanguageTalks }.SelectMany(x => x)).ConfigureAwait(false);

        var sb = new StringBuilder();
        var oldLanguageIndices = oldLanguages.Keys.ToHashSet();
        oldLanguageIndices.ExceptWith(this._languageIndices);
        sb.AppendLine("[Deprecated Languages]");
        foreach (var oldLanguageIndex in oldLanguageIndices)
        {
            sb.AppendLine($"  {oldLanguageIndex}");
        }
        sb.AppendLine();

        var curLanguageIndices = this._languageIndices.ToHashSet();
        curLanguageIndices.ExceptWith(oldLanguages.Keys);
        sb.AppendLine("[Appended Languages]");
        foreach (var curLanguageIndex in curLanguageIndices)
        {
            sb.AppendLine($"  {curLanguageIndex}");
        }
        sb.AppendLine();

        var sharedLanguageIndices = oldLanguages.Keys.ToHashSet();
        sharedLanguageIndices.IntersectWith(this._languageIndices);
        sb.AppendLine("[Updated Languages]");
        foreach (var sharedIndex in sharedLanguageIndices)
        {
            if (oldLanguages.TryGetValue(sharedIndex, out var oldText)
                && this._containerCache.TryGetValue(new KeyNotification
                {
                    KeyType = LanguageKeyTypes.Language,
                    Key = sharedIndex,
                }, out var curText)
                && oldText != curText.SimpleChinese)
            {
                sb.AppendLine($"<{sharedIndex}>---");
                sb.AppendLine(oldText);
                sb.AppendLine("↓");
                sb.AppendLine(curText.SimpleChinese);
                sb.AppendLine("---");
            }
        }
        sb.AppendLine();

        var oldLanguageTalkIndices = oldLanguageTalks.Keys.ToHashSet();
        oldLanguageTalkIndices.ExceptWith(this._languageTalkIndices);
        sb.AppendLine("[Deprecated LanguageTalks]");
        foreach (var oldLanguageTalkIndex in oldLanguageTalkIndices)
        {
            sb.AppendLine($"  {oldLanguageTalkIndex}");
        }
        sb.AppendLine();

        var curLanguageTalkIndices = this._languageTalkIndices.ToHashSet();
        curLanguageTalkIndices.ExceptWith(oldLanguageTalks.Keys);
        sb.AppendLine("[Appended LanguageTalks]");
        foreach (var curLanguageTalkIndex in curLanguageTalkIndices)
        {
            sb.AppendLine($"  {curLanguageTalkIndex}");
        }
        sb.AppendLine();

        var sharedLanguageTalkIndices = oldLanguageTalks.Keys.ToHashSet();
        sharedLanguageTalkIndices.IntersectWith(this._languageTalkIndices);
        sb.AppendLine("[Updated LanguageTalks]");
        foreach (var sharedIndex in sharedLanguageTalkIndices)
        {
            if (oldLanguageTalks.TryGetValue(sharedIndex, out var oldText)
                && this._containerCache.TryGetValue(new KeyNotification
                {
                    KeyType = LanguageKeyTypes.LanguageTalk,
                    Key = sharedIndex,
                }, out var curText)
                && oldText != curText.SimpleChinese)
            {
                sb.AppendLine($"<{sharedIndex}>---");
                sb.AppendLine(oldText);
                sb.AppendLine("↓");
                sb.AppendLine(curText.SimpleChinese);
                sb.AppendLine("---");
            }
        }
        sb.AppendLine();

        var reportPath = Path.Combine(this._pathDef.PluginRootPath, "diff_Originals.txt");
        File.WriteAllText(reportPath, sb.ToString(), Encoding.UTF8);
    }

    public void Dispose()
	{
		this._cts.Cancel();
		this._cts.Dispose();
		this._lock.Dispose();
		GC.SuppressFinalize(this);
	}
}
