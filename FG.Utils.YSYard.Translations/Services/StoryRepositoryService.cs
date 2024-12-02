using FG.Defs.YSYard.Translations.Devs;
using FG.Utils.YSYard.Translations.Contracts.Models;
using FG.Utils.YSYard.Translations.Contracts.Services;
using FG.Utils.YSYard.Translations.Helpers;
using FG.Utils.YSYard.Translations.Models;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;

namespace FG.Utils.YSYard.Translations.Services;

public class StoryRepositoryService : IStoryRepositoryService, IDisposable
{
    private readonly IWritableOptions<AppConfig> _appConfig;

    private readonly ReaderWriterLockSlim _lock = new();

    private DevelopmentPathDefs _pathDef = null!;

    private readonly List<int> _storyIndices = [];

    private readonly Dictionary<int, string> _storyRefs = [];

    private readonly Dictionary<int, StoryContainer> _containerCache = [];

    private bool _isFullyLoaded = false;

    private readonly CancellationTokenSource _cts = new();

    public StoryRepositoryService(IWritableOptions<AppConfig> appConfig)
    {
        this._appConfig = appConfig;
        this.SetPluginFolderPath(this._appConfig.Value.PluginFolderPath);
    }

    public void SetPluginFolderPath(string pluginFolderPath)
    {
        this._storyIndices.Clear();
        this._storyRefs.Clear();

        this._pathDef = new(pluginFolderPath);
        if (!this._pathDef.IsValid)
        {
            return;
        }
        this._pathDef.EnsureAllCreated();

        foreach (var file in Directory.EnumerateFiles(this._pathDef.StoriesPath))
        {
            if (this._pathDef.TryGetKeyFromPath(file, out var key))
            {
                this._storyRefs[key] = file;
            }
        }
        this._storyIndices.AddRange(this._storyRefs.Keys);
        this._storyIndices.Sort();
    }

    public bool TryGetStory(int id, [MaybeNullWhen(false)] out StoryContainer story)
    {
        story = null;
        if (this._containerCache.TryGetValue(id, out var container))
        {
            story = container;
            return true;
        }

        if (this._storyRefs.TryGetValue(id, out var path))
        {
            return this.TryLoadStoryContainer(path, out story);
        }
        return false;
    }

    private bool TryLoadStoryContainer(string path, [MaybeNullWhen(false)] out StoryContainer story)
    {
        story = null;
        var json = File.ReadAllText(path, Encoding.UTF8);
        if (string.IsNullOrEmpty(json))
        {
            return false;
        }
        var tmp = JsonSerializer.Deserialize<StoryContainer>(json);
        if (tmp == null)
        {
            return false;
        }
        using var wl = new WriteLock(this._lock);
        story = tmp;
        this._containerCache[tmp.Id] = tmp;
        return true;
    }

    public bool TryGetNextStory(int id, int indexOffset, [MaybeNullWhen(false)] out StoryContainer story)
    {
        story = null;

        var idx = this._storyIndices.IndexOf(id);
        if (idx < 0)
        {
            return false;
        }

        var nextIdx = idx + indexOffset;
        if (nextIdx < 0)
        {
            return this.TryGetStory(this._storyIndices.First(), out story);
        }
        else if (this._storyIndices.Count <= nextIdx)
        {
            return this.TryGetStory(this._storyIndices.Last(), out story);
        }
        else
        {
            return this.TryGetStory(this._storyIndices[nextIdx], out story);
        }
    }

    public async Task<StoryContainer?> SearchStoryAsync(int languageTalkKey)
    {
        await this.LoadAllStoriesAsync().ConfigureAwait(false);

        if (this._cts.IsCancellationRequested)
        {
            return null;
        }

        foreach (var cache in this._containerCache)
        {
            foreach (var cc in cache.Value.Conversations)
            {
                if (this._cts.IsCancellationRequested)
                {
                    return null;
                }
                if (cc.SentenceKey == languageTalkKey)
                {
                    return cache.Value;
                }
            }
        }
        return null;
    }

    private async Task LoadAllStoriesAsync()
    {
        if (this._isFullyLoaded)
        {
            return;
        }

        var taskStories = this._storyRefs.Select(x => Task.Run(() =>
        {
            if (this._cts.IsCancellationRequested)
            {
                return;
            }
            this.TryLoadStoryContainer(x.Value, out _);
        }));
        await Task.WhenAll(taskStories).ConfigureAwait(false);

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
