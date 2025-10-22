using FG.Defs.YSYard.Translations;
using FG.Defs.YSYard.Translations.Devs;
using FG.Utils.YSYard.Translations.Contracts.Models;
using FG.Utils.YSYard.Translations.Enums;
using FG.Utils.YSYard.Translations.Models;
using FG.Utils.YSYard.Translations.Services;
using Microsoft.AspNetCore.Components.Web;

namespace FG.Utils.YSYard.Translations.ViewModels;

public class TranslationPageViewModel(
    IWritableOptions<AppConfig> appConfig,
    CustomDialogService customDialog,
    SharedSnackbarService snackbar,
    KeyNotificationService keyNotification,
    TranslationStoreService tlStore,
    TranslationApiService translationApi,
    IgnoreListService ignoreList,
    StoryStoreService storyStore)
    : ViewModelBase, IDisposable
{
    public string PluginMainDirPath
    {
        get; set;
    } = string.IsNullOrEmpty(appConfig.Value.PluginMainDirPath)
        ? "クリックして選択" : appConfig.Value.PluginMainDirPath;

    public string PluginDevDirPath
    {
        get; set;
    } = string.IsNullOrEmpty(appConfig.Value.PluginDevDirPath)
        ? "クリックして選択" : appConfig.Value.PluginDevDirPath;

    public bool PluginIsFound
    {
        get; set;
    }

    private LanguageKeyTypes? _filterType;
    public LanguageKeyTypes? FilterType
    {
        get => this._filterType;
        set
        {
            var changed = this._filterType != value;
            this._filterType = value;
            if (changed)
            {
                keyNotification.ForceNotifyAll();
            }
        }
    }

    private bool _filterTranslated;
    public bool FilterTranslated
    {
        get => this._filterTranslated;
        set
        {
            var changed = this._filterTranslated != value;
            this._filterTranslated = value;
            if (changed)
            {
                keyNotification.ForceNotifyAll();
            }
        }
    }

    private bool _filterIgnored;
    public bool FilterIgnored
    {
        get => this._filterIgnored;
        set
        {
            var changed = this._filterIgnored != value;
            this._filterIgnored = value;
            if (changed)
            {
                keyNotification.ForceNotifyAll();
            }
        }
    }

    public List<StagingLanguageContainerViewModel> NotifiedLanguages
    {
        get;
    } = [];

    public StagingLanguageContainerViewModel? SelectedSLC
    {
        get; set;
    }

    public string SpeakerName
    {
        get; set;
    } = string.Empty;

    public string CurrentUserTranslation
    {
        get; set;
    } = string.Empty;

    public string CurrentOriginal
    {
        get; set;
    } = string.Empty;

    public string CurrentEnglish
    {
        get; set;
    } = string.Empty;

    public string CurrentPlaceholder
    {
        get; set;
    } = string.Empty;

    public string CurrentApiPretranslated
    {
        get; set;
    } = string.Empty;

    public bool UseTranslationApi
    {
        get; set;
    }

    public string ApiTranslatedString
    {
        get; set;
    } = string.Empty;

    private LanguageKeyTypes? _selectedType;
    public LanguageKeyTypes? SelectedType
    {
        get => this._selectedType;
        set
        {
            var changed = this._selectedType != value;
            this._selectedType = value;
            if (changed)
            {
                this.SelectContainerByKeyType();
            }
        }
    }

    public int? SelectedKey
    {
        get; set;
    }

    public SearchRanges SelectedSearchRange
    {
        get; set;
    }

    public string TextSearchPatternString
    {
        get; set;
    } = string.Empty;

    public string? SelectedStory
    {
        get; set;
    }

    public int? SelectedStorySentenceKey
    {
        get; set;
    }

    public bool UsePlaceholderMode
    {
        get; set;
    }

    public bool UseSrcAsComparisonBase
    {
        get; set;
    }

    protected override void OnPageEnterInternal()
    {
        this.PluginIsFound =
            new LanguagePathDefs(this.PluginMainDirPath).IsValid
            && new DevelopmentPathDefs(this.PluginDevDirPath).IsValid;
        keyNotification.RegisterCallback(this.OnKeyNotified);
    }

    public void SetPluginMainDir() => this.InvokeAsync(async () =>
    {
        var path = await customDialog.OpenFolderAsync();
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        this.PluginMainDirPath = path;
        this.PluginIsFound =
            new LanguagePathDefs(this.PluginMainDirPath).IsValid
            && new DevelopmentPathDefs(this.PluginDevDirPath).IsValid;
        tlStore.SetOutputDir(this.PluginMainDirPath);
        appConfig.Update(x =>
        {
            x.PluginMainDirPath = this.PluginMainDirPath;
        });
    });

    public void SetPluginDevDir() => this.InvokeAsync(async () =>
    {
        var path = await customDialog.OpenFolderAsync();
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        this.PluginDevDirPath = path;
        this.PluginIsFound =
            new LanguagePathDefs(this.PluginMainDirPath).IsValid
            && new DevelopmentPathDefs(this.PluginDevDirPath).IsValid;
        tlStore.SetStagingDir(this.PluginDevDirPath);
        appConfig.Update(x =>
        {
            x.PluginDevDirPath = this.PluginDevDirPath;
        });
    });

    public void ClearNotifiedLanguages()
    {
        keyNotification.ClearAll();
    }

    public void SelectLanguageContainer(StagingLanguageContainerViewModel vm)
    {
        this.SelectedSLC = vm;
        this.CurrentOriginal = vm.Raw.Original;
        this.CurrentEnglish = vm.Raw.English;
        this.CurrentPlaceholder = vm.Raw.Placeholder;
        this.CurrentApiPretranslated = vm.Raw.ApiTranslated;
        this.CurrentUserTranslation = vm.Raw.Japanese;
        this._selectedType = vm.NotifiedKey.KeyType;
        this.SelectedKey = vm.NotifiedKey.Key;
        if (this.UseTranslationApi && !string.IsNullOrEmpty(this.CurrentOriginal))
        {
            this.InvokeAsync(async () =>
            {
                this.ApiTranslatedString = await translationApi.TranslateAsync(this.CurrentOriginal);
            });
        }
    }

    public void ToggleIgnoreState(StagingLanguageContainerViewModel vm)
    {
        if (vm.IsIgnored)
        {
            ignoreList.Remove(vm.NotifiedKey);
            vm.IsIgnored = false;
        }
        else
        {
            ignoreList.Add(vm.NotifiedKey);
            vm.IsIgnored = true;
        }
        if (this.FilterIgnored)
        {
            keyNotification.ForceNotifyAll();
        }
    }

    public void RemoveNotification(StagingLanguageContainerViewModel vm)
        => keyNotification.Remove(vm.NotifiedKey);

    public void OnKeyDownInUserTranslation(KeyboardEventArgs e)
    {
        if (e.CtrlKey && e.Key == "s")
        {
            if (this.SelectedSLC == null)
            {
                return;
            }
            this.SelectedSLC.UpdateUserTranslation(this.CurrentUserTranslation);
            tlStore.RequestTemporarySave();
            keyNotification.Remove(this.SelectedSLC.NotifiedKey);
        }
    }

    public void SelectPreviousContainer()
        => this.SelectOffsetContainer(-1);

    public void SelectNextContainer()
        => this.SelectOffsetContainer(1);

    public void SearchTextPattern()
    {
        if (!this.PluginIsFound)
        {
            snackbar.Snackbar?.Add(
                "プラグインディレクトリがセットされていないか, 有効ではありません.",
                MudBlazor.Severity.Error);
            return;
        }

        this.InvokeAsync(async () =>
        {
            var items = await tlStore
                .SearchTextAsync(this.SelectedSearchRange, this.TextSearchPatternString)
                .ConfigureAwait(false);
            keyNotification.ForceNotify(items);
            snackbar.Snackbar?.Add($"{items.Count}件見つかりました.");
        });
    }

    public void SelectPreviousStory()
        => this.SelectOffsetStory(-1);

    public void SelectNextStory()
        => this.SelectOffsetStory(1);

    public void SelectPreviousStoryTalk()
        => this.SelectOffsetStoryTalk(-1);

    public void SelectNextStoryTalk()
        => this.SelectOffsetStoryTalk(1);

    public void SelectStoryBySelectedContainer()
    {
        if (this.SelectedSLC == null)
        {
            return;
        }
        if (!storyStore.TryGetStory(this.SelectedSLC.NotifiedKey.Key, out var story))
        {
            return;
        }

        this.SetStoryProperties(
            story.storyName, story.talkPair.SentenceKey, story.talkPair.SpeakerKey);
    }

    public void ExportDocx()
    {
        if (!this.PluginIsFound)
        {
            snackbar.Snackbar?.Add(
                "プラグインディレクトリがセットされていないか, 有効ではありません.",
                MudBlazor.Severity.Error);
            return;
        }

        this.InvokeAsync(async () =>
        {
            var opts = new FilePickerOptions();
            opts.Filters["Word文書ファイル"] = [".docx"];
            var path = await customDialog.SaveFileAsync(opts);
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            tlStore.ExportDocx(path);
            snackbar.Snackbar?.Add($"出力完了: {path}");
        });
    }

    public void ImportDocx()
    {
        if (!this.PluginIsFound)
        {
            snackbar.Snackbar?.Add(
                "プラグインディレクトリがセットされていないか, 有効ではありません.",
                MudBlazor.Severity.Error);
            return;
        }

        this.InvokeAsync(async () =>
        {
            var opts = new FilePickerOptions();
            opts.Filters["Word文書ファイル"] = [".docx"];
            var path = await customDialog.OpenFileAsync(opts);
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            tlStore.ImportDocx(path);
            snackbar.Snackbar?.Add($"入力完了: {path}");
        });
    }

    public void SerializeTranslations()
    {
        Task.Run(() =>
        {
            tlStore.SerializeTranslations(
                this.UsePlaceholderMode);
            snackbar.Snackbar?.Add($"メインプラグインディレクトリに翻訳を書き出しました.");
        });
    }

    public void ReportDiff() => this.InvokeAsync(async () =>
    {
        var opts = new FilePickerOptions();
        opts.Filters["ステージングファイル"] = [".json", ".tmp"];
        var srcPath = await customDialog.OpenFileAsync(opts);
        if (string.IsNullOrEmpty(srcPath))
        {
            return;
        }

        opts.Filters.Clear();
        opts.Filters["Markdownファイル"] = [".md"];
        var dstPath = await customDialog.SaveFileAsync(opts);
        if (string.IsNullOrEmpty(dstPath))
        {
            return;
        }

        tlStore.ReportStagingDiff(srcPath, dstPath, this.UseSrcAsComparisonBase);
        snackbar.Snackbar?.Add($"レポートを出力しました: {dstPath}");
    });

    public void UpdateCacheWithStagingFile() => this.InvokeAsync(async () =>
    {
        var opts = new FilePickerOptions();
        opts.Filters["ステージングファイル"] = [".json"];
        var srcPath = await customDialog.OpenFileAsync(opts);
        if (string.IsNullOrEmpty(srcPath))
        {
            return;
        }

        tlStore.UpdateCacheWithStagingFile(srcPath);
        keyNotification.ForceNotifyAll();
        snackbar.Snackbar?.Add("新たなステージングファイルでキャッシュを更新しました");
    });

    private void OnKeyNotified(List<LanguageKey> kns) => this.InvokeAsync(() =>
    {
        this.NotifiedLanguages.Clear();
        foreach (var kn in kns)
        {
            if (!tlStore.TryGetContainer(kn, out var container))
            {
                continue;
            }
            if (this.FilterType != null
                && kn.KeyType != this.FilterType)
            {
                continue;
            }
            if (this.FilterTranslated && container.IsTranslated)
            {
                continue;
            }
            var isIgnored = ignoreList.IsIgnored(kn);
            if (this.FilterIgnored && isIgnored)
            {
                continue;
            }

            this.NotifiedLanguages.Add(new(container, kn, isIgnored));
        }
    });

    private void SelectContainerByKeyType() => this.InvokeAsync(() =>
    {
        if (this._selectedType == null)
        {
            return;
        }
        if (!tlStore.TryGetKeyTypeFirstContainer(this._selectedType.Value, out var tup))
        {
            return;
        }

        if (ignoreList.IsIgnored(tup.kn))
        {
            snackbar.Snackbar?.Add("その翻訳キーは非表示に設定されています.", MudBlazor.Severity.Warning);
        }
        this.SelectLanguageContainer(new(tup.container, tup.kn, false));
    });

    private void SelectOffsetContainer(int offset)
    {
        if (this.SelectedSLC == null)
        {
            return;
        }
        if (!tlStore.TryGetNextContainer(
            this.SelectedSLC.NotifiedKey, offset, out var tup))
        {
            return;
        }
        if (ignoreList.IsIgnored(tup.nextKn))
        {
            snackbar.Snackbar?.Add("その翻訳キーは非表示に設定されています.", MudBlazor.Severity.Warning);
        }

        this.SelectLanguageContainer(new(tup.container, tup.nextKn, false));
    }

    private void SetStoryProperties(string? storyName, int? sentenceKey, int? speakerKey)
    {
        this.SelectedStory = storyName;
        this.SelectedStorySentenceKey = sentenceKey;
        if (speakerKey == null
            || !tlStore.TryGetTalkContainer(speakerKey.Value, out var tup))
        {
            this.SpeakerName = string.Empty;
        }
        else
        {
            this.SpeakerName =
                string.IsNullOrEmpty(tup.container.Japanese)
                ? tup.container.Original
                : tup.container.Japanese;
        }
    }

    private void SelectOffsetStory(int offset)
    {
        var nextStoryName = string.Empty;
        var nextSentenceKey = -1;
        var nextSpeakerKey = -1;
        if (string.IsNullOrEmpty(this.SelectedStory))
        {
            if (storyStore.TryGetPlaceholderStory(out var story))
            {
                nextStoryName = story.storyName;
                nextSentenceKey = story.talkPair.SentenceKey;
                nextSpeakerKey = story.talkPair.SpeakerKey;
            }
        }
        else if (storyStore.TryGetNextStory(
            this.SelectedStory, offset, out var story))
        {
            nextStoryName = story.storyName;
            nextSentenceKey = story.talkPair.SentenceKey;
            nextSpeakerKey = story.talkPair.SpeakerKey;
        }
        if (string.IsNullOrEmpty(nextStoryName)
            || !tlStore.TryGetTalkContainer(nextSentenceKey, out var tup))
        {
            snackbar.Snackbar?.Add("ストーリーのセットに失敗しました.", MudBlazor.Severity.Warning);
            return;
        }

        if (ignoreList.IsIgnored(tup.kn))
        {
            snackbar.Snackbar?.Add("その翻訳キーは非表示に設定されています.", MudBlazor.Severity.Warning);
        }
        this.SetStoryProperties(nextStoryName, nextSentenceKey, nextSpeakerKey);
        this.SelectLanguageContainer(new(tup.container, tup.kn, false));
    }

    private void SelectOffsetStoryTalk(int offset)
    {
        var nextStoryName = string.Empty;
        var nextSentenceKey = -1;
        var nextSpeakerKey = -1;
        if (string.IsNullOrEmpty(this.SelectedStory))
        {
            if (storyStore.TryGetPlaceholderStory(out var story))
            {
                nextStoryName = story.storyName;
                nextSentenceKey = story.talkPair.SentenceKey;
                nextSpeakerKey = story.talkPair.SpeakerKey;
            }
        }
        else if (this.SelectedStorySentenceKey != null
            && storyStore.TryGetNextStoryTalkPair(
                this.SelectedStory, this.SelectedStorySentenceKey.Value, offset,
                out var nextPair))
        {
            nextStoryName = this.SelectedStory;
            nextSentenceKey = nextPair.SentenceKey;
            nextSpeakerKey = nextPair.SpeakerKey;
        }
        if (string.IsNullOrEmpty(nextStoryName)
            || !tlStore.TryGetTalkContainer(nextSentenceKey, out var tup))
        {
            snackbar.Snackbar?.Add("ストーリーのセットに失敗しました.", MudBlazor.Severity.Warning);
            return;
        }

        if (ignoreList.IsIgnored(tup.kn))
        {
            snackbar.Snackbar?.Add("その翻訳キーは非表示に設定されています.", MudBlazor.Severity.Warning);
        }
        this.SetStoryProperties(nextStoryName, nextSentenceKey, nextSpeakerKey);
        this.SelectLanguageContainer(new(tup.container, tup.kn, false));
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
