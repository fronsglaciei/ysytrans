using FG.Defs.YSYard.Translations.Devs;
using FG.Utils.YSYard.Translations.Contracts.Models;
using FG.Utils.YSYard.Translations.Interops;
using FG.Utils.YSYard.Translations.Models;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace FG.Utils.YSYard.Translations.Services;

public class StoryStoreService
{
    private readonly IWritableOptions<AppConfig> _appConfig;

    private DevelopmentPathDefs _devPathDef = null!;

    private StoryDictionary _storyDict = new();

    private readonly List<string> _indexToStoryNames = [];

    private readonly Dictionary<string, int> _storyNameToIndices = [];

    private readonly Dictionary<string, Dictionary<int, int>> _keyToIndexMap = [];

    public StoryStoreService(
        IWritableOptions<AppConfig> appConfig)
    {
        this._appConfig = appConfig;
        this.SetStagingDir(this._appConfig.Value.PluginDevDirPath);
    }

    public void SetStagingDir(string stagingDir)
    {
        this._devPathDef = new(stagingDir);
        if (!this._devPathDef.IsValid)
        {
            return;
        }
        this.LoadDictionary();
    }

    public bool TryGetStory(
        int languageTalkKey,
        [MaybeNullWhen(false)] out (string storyName, StoryTalkPair talkPair) story)
    {
        story = default;
        foreach (var kvp in this._storyDict.Dict)
        {
            foreach (var pair in kvp.Value)
            {
                if (pair.SentenceKey == languageTalkKey)
                {
                    story = (kvp.Key, pair);
                    return true;
                }
            }
        }
        return false;
    }

    public bool TryGetStory(
        LanguageKey kn,
        [MaybeNullWhen(false)] out (string storyName, StoryTalkPair talkPair) story)
    {
        story = default;
        if (kn.KeyType != LanguageKeyTypes.LanguageTalk)
        {
            return false;
        }
        return this.TryGetStory(kn.Key, out story);
    }

    public bool TryGetStoryFirstPair(
        string storyName, [MaybeNullWhen(false)] out StoryTalkPair talkPair)
    {
        talkPair = default;
        if (!this._storyDict.Dict.TryGetValue(storyName, out var pairs))
        {
            return false;
        }
        if (pairs.Count < 1)
        {
            return false;
        }
        talkPair = pairs[0];
        return true;
    }

    public bool TryGetPlaceholderStory(
        [MaybeNullWhen(false)] out (string storyName, StoryTalkPair talkPair) story)
    {
        story = default;
        if (this._storyDict.Dict.Count < 1)
        {
            return false;
        }
        foreach (var kvp in this._storyDict.Dict)
        {
            if (0 < kvp.Value.Count)
            {
                story = (kvp.Key, kvp.Value[0]);
                return true;
            }
        }
        return false;
    }

    public bool TryGetNextStory(
        string storyName, int offset,
        [MaybeNullWhen(false)] out (string storyName, StoryTalkPair talkPair) nextStory)
    {
        nextStory = default;
        if (!this._storyNameToIndices.TryGetValue(storyName, out var curIdx))
        {
            return false;
        }

        var nextIdx = Math.Min(
            Math.Max(0, curIdx + offset),
            this._indexToStoryNames.Count - 1);
        var nextStoryName = this._indexToStoryNames[nextIdx];
        nextStory = (
            nextStoryName,
            this._storyDict.Dict[nextStoryName][0]);
        return true;
    }

    public bool TryGetNextStoryTalkPair(
        string storyName, int languageTalkKey, int offset,
        [MaybeNullWhen(false)] out StoryTalkPair nextPair)
    {
        nextPair = null;
        if (!this._keyToIndexMap.TryGetValue(storyName, out var dictTalkIdx))
        {
            return false;
        }
        if (!dictTalkIdx.TryGetValue(languageTalkKey, out var curIdx))
        {
            return false;
        }
        if (!this._storyDict.Dict.TryGetValue(storyName, out var listPairs)
            || listPairs.Count < 1)
        {
            return false;
        }

        var nextIdx = Math.Min(Math.Max(0, curIdx + offset), listPairs.Count - 1);
        nextPair = listPairs[nextIdx];
        return true;
    }

    private void LoadDictionary()
    {
        var json = File.ReadAllText(this._devPathDef.StoryPath);
        var tmpStory = JsonSerializer.Deserialize<StoryDictionary>(json);
        if (tmpStory == null)
        {
            return;
        }
        this._storyDict = tmpStory;

        this._indexToStoryNames.Clear();
        var tmpStoryNames = this._storyDict.Dict.Keys.ToList();
        tmpStoryNames.Sort(NativeMethods.LogicalCompare);
        this._indexToStoryNames.AddRange(tmpStoryNames);

        this._storyNameToIndices.Clear();
        for (var i = 0; i < this._indexToStoryNames.Count; i++)
        {
            this._storyNameToIndices[this._indexToStoryNames[i]] = i;
        }

        this._keyToIndexMap.Clear();
        foreach (var kvp in this._storyDict.Dict)
        {
            for (var i = 0; i < kvp.Value.Count; i++)
            {
                if (i == 0)
                {
                    this._keyToIndexMap[kvp.Key] = [];
                }
                this._keyToIndexMap[kvp.Key][kvp.Value[i].SentenceKey] = i;
            }
        }
    }
}
