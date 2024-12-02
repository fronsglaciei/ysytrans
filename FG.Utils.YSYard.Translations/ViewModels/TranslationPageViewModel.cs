using FG.Defs.YSYard.Translations.Devs;
using FG.Utils.YSYard.Translations.Contracts.Models;
using FG.Utils.YSYard.Translations.Contracts.Services;
using FG.Utils.YSYard.Translations.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Collections.Concurrent;

namespace FG.Utils.YSYard.Translations.ViewModels;

public class TranslationPageViewModel : ComponentBase, IDisposable
{
    [Inject]
    protected IWritableOptions<AppConfig> AppConfig { get; set; } = null!;

    [Inject]
    protected ICustomDialogService CustomDialog { get; set; } = null!;

    [Inject]
    protected ISnackbar Snackbar { get; set; } = null!;

    [Inject]
    protected IKeyNotificationService KeyNotification { get; set; } = null!;

    [Inject]
    protected ILanguageRepositoryService LanguageRepository { get; set; } = null!;

    [Inject]
    protected ITranslationRepositoryService TranslationRepository { get; set; } = null!;

    [Inject]
    protected IStoryRepositoryService StoryRepository { get; set; } = null!;

    [Inject]
    protected ITranslationApiService TranslationApi { get; set; } = null!;

    [Inject]
    protected IIgnoreListService IgnoreList { get; set; } = null!;

    protected string PluginFolderPath { get; set; } = "クリックして選択";

    protected bool PluginIsFound { get; set; } = false;

    private LanguageKeyTypes _selectedNotificationFilter;
    protected LanguageKeyTypes SelectedNotificationFilter
    {
        get => this._selectedNotificationFilter;
        set
        {
            var isChanged = this._selectedNotificationFilter != value;
            this._selectedNotificationFilter = value;
            if (isChanged)
            {
                this.UpdateNotifiedLanguages();
            }
        }
    }

    private bool _isFilterTranslated;
    protected bool IsFilterTranslated
    {
        get => this._isFilterTranslated;
        set
        {
            var isChanged = this._isFilterTranslated != value;
            this._isFilterTranslated = value;
            if (isChanged)
            {
                this.UpdateNotifiedLanguages();
            }
        }
    }

    private bool _isFilterIgnored;
    protected bool IsFilterIgnored
    {
        get => this._isFilterIgnored;
        set
        {
            var isChanged = this._isFilterIgnored != value;
            this._isFilterIgnored = value;
            if (isChanged)
            {
                this.UpdateNotifiedLanguages();
            }
        }
    }

    protected string SpeakerName { get; set; } = string.Empty;

    protected bool IsTranslationApiUsed { get; set; } = true;

    protected ConcurrentStack<LanguageContainerViewModel> NotifiedLanguages { get; } = new();

    protected TranslationWorkViewModel SelectedTranslationWork { get; set; } = new(LanguageContainer.Empty, string.Empty, _ => { });

    protected string ApiTranslatedString { get; set; } = string.Empty;

    protected KeyNotification SelectedTranslationKeyNotification { get; set; } = new();

    protected string LanguageSearchPattern { get; set; } = string.Empty;

    protected string TranslationSearchPattern { get; set; } = string.Empty;

    private StoryContainer? _selectedStory;
    protected int SelectedStoryId { get; set; }

    protected int SelectedStoryIndex { get; set; }

    protected override void OnInitialized()
    {
        this.PluginFolderPath = this.AppConfig.Value.PluginFolderPath;

        this.PluginIsFound = new DevelopmentPathDefs(this.PluginFolderPath).IsValid;

        this.KeyNotification.KeyNotifications.CollectionChanged += KeyNotifications_CollectionChanged;

        base.OnInitialized();
    }

    private void KeyNotifications_CollectionChanged(object? _, System.Collections.Specialized.NotifyCollectionChangedEventArgs __)
        => this.UpdateNotifiedLanguages();

    protected void SetPluginFolder() => this.InvokeAsync(async () =>
    {
        var path = await this.CustomDialog.OpenFolderAsync();
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        this.PluginFolderPath = path;
        this.PluginIsFound = new DevelopmentPathDefs(path).IsValid;
        this.LanguageRepository.SetPluginFolderPath(path);
        this.TranslationRepository.SetPluginFolderPath(path);
        this.AppConfig.Update(x =>
        {
            x.PluginFolderPath = this.PluginFolderPath;
        });
        this.StateHasChanged();
    });

    protected void ClearNotifiedLanguages()
    {
        this.KeyNotification.ClearAll();
        this.StateHasChanged();
    }

    protected void SetTranslationWorkBySelectedKey()
    {
        if (this.LanguageRepository.TryGetContainer(this.SelectedTranslationKeyNotification, out var lc))
        {
            this.UpdateSelectedTranslationWork(lc);
        }
        else
        {
            this.SelectedTranslationWork = new(LanguageContainer.Empty, string.Empty, _ => { });
        }
    }

    protected void SetPreviousTranslationWork()
    {
        if (this.SelectedTranslationWork.KeyType == LanguageKeyTypes.Undefined)
        {
            return;
        }

        if (!this.LanguageRepository.TryGetNextContainer(this.SelectedTranslationWork.KeyNotification, -1, out var lc))
        {
            return;
        }

        this.UpdateSelectedTranslationWork(lc);
    }

    protected void SetNextTranslationWork()
    {
        if (this.SelectedTranslationWork.KeyType == LanguageKeyTypes.Undefined)
        {
            return;
        }

        if (!this.LanguageRepository.TryGetNextContainer(this.SelectedTranslationWork.KeyNotification, 1, out var lc))
        {
            return;
        }

        this.UpdateSelectedTranslationWork(lc);
    }

    protected void SetTranslationWorkByStory()
    {
        if (this.StoryRepository.TryGetStory(this.SelectedStoryId, out var sc))
        {
            this.SetStory(sc);
        }
    }

    protected void SetPreviousStory()
    {
        if (this.StoryRepository.TryGetNextStory(this.SelectedStoryId, -1, out var sc))
        {
            this.SelectedStoryId = sc.Id;
            this.SetStory(sc);
        }
    }

    protected void SetNextStory()
    {
        if (this.StoryRepository.TryGetNextStory(this.SelectedStoryId, 1, out var sc))
        {
            this.SelectedStoryId = sc.Id;
            this.SetStory(sc);
        }
    }

    protected void SetPreviousStoryIndex() => this.SetStoryIndexByOffset(-1);

    protected void SetNextStoryIndex() => this.SetStoryIndexByOffset(1);

    protected void SearchStoryByLanguageTalk()
    {
        if (!this.PluginIsFound)
        {
            this.Snackbar.Add("プラグインフォルダがセットされていないか, 有効ではありません.", Severity.Error);
            return;
        }
        if (this.SelectedTranslationKeyNotification.KeyType != LanguageKeyTypes.LanguageTalk)
        {
            return;
        }

        Task.Run(async () =>
        {
            var key = this.SelectedTranslationKeyNotification.Key;
            var sc = await this.StoryRepository.SearchStoryAsync(key).ConfigureAwait(false);
            if (sc == null)
            {
                return;
            }
            this.SelectedStoryId = sc.Id;
            this.SelectedStoryIndex = sc.Conversations.FindIndex(x => x.SentenceKey == key);
            this.SetStory(sc);
        });
    }

    protected void SearchLanguagePattern()
    {
        if (!this.PluginIsFound)
        {
            this.Snackbar.Add("プラグインフォルダがセットされていないか, 有効ではありません.", Severity.Error);
            return;
        }

        Task.Run(async () =>
        {
            var searchedItems = await this.LanguageRepository.SearchContainersAsync(this.LanguageSearchPattern).ConfigureAwait(false);
            foreach (var item in searchedItems)
            {
                item.TimeStamp = DateTime.Now;
            }
            this.KeyNotification.ForceNotify(searchedItems);
            this.Snackbar.Add($"{searchedItems.Count()}件見つかりました.");
        });
    }

    protected void SearchTranslationPattern()
    {
        if (!this.PluginIsFound)
        {
            this.Snackbar.Add("プラグインフォルダがセットされていないか, 有効ではありません.", Severity.Error);
            return;
        }

        Task.Run(async () =>
        {
            var searchedItems = await this.TranslationRepository.SearchTranslationAsync(this.TranslationSearchPattern).ConfigureAwait(false);
            foreach (var item in searchedItems)
            {
                item.TimeStamp = DateTime.Now;
            }
            this.KeyNotification.ForceNotify(searchedItems);
            this.Snackbar.Add($"{searchedItems.Count()}件見つかりました.");
        });
    }

    protected void SaveBackup()
    {
        if (!this.PluginIsFound)
        {
            this.Snackbar.Add("プラグインフォルダがセットされていないか, 有効ではありません.", Severity.Error);
            return;
        }

        Task.Run(async () =>
        {
            var opts = new FilePickerOptions();
            opts.Filters["JSONファイル"] = [".json"];
            var path = await this.CustomDialog.SaveFileAsync(opts);
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            var json = await this.TranslationRepository.SaveBackupAsync(path).ConfigureAwait(false);
            this.Snackbar.Add("保存完了", Severity.Info);
        });
    }

    protected void LoadBackup()
    {
        if (!this.PluginIsFound)
        {
            this.Snackbar.Add("プラグインフォルダがセットされていないか, 有効ではありません.", Severity.Error);
            return;
        }

        this.InvokeAsync(async () =>
        {
            var isConfirmed = await this.CustomDialog.ConfirmAsync(
                "バックアップをロードすると現在の翻訳ファイル群は上書きされます. よろしいですか？", null, "実行");
            if (!isConfirmed)
            {
                return;
            }

            var opts = new FilePickerOptions();
            opts.Filters["JSONファイル"] = [".json"];
            var path = await this.CustomDialog.OpenFileAsync(opts);
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            await this.TranslationRepository.LoadBackupAsync(path).ConfigureAwait(false);
            this.Snackbar.Add("上書き完了", Severity.Info);
        });
    }

    protected void SerializeTranslations()
    {
        if (!this.PluginIsFound)
        {
            this.Snackbar.Add("プラグインフォルダがセットされていないか, 有効ではありません.", Severity.Error);
            return;
        }

        Task.Run(async () =>
        {
            var res = await this.TranslationRepository.SerializeAsync().ConfigureAwait(false);
            if (res)
            {
                this.Snackbar.Add("保存完了", Severity.Info);
            }
            else
            {
                this.Snackbar.Add("保存失敗", Severity.Error);
            }
        });
    }

    protected void ReportDiff() => this.InvokeAsync(async () =>
    {
        var isConfirmed = await this.CustomDialog.ConfirmAsync(
            "現在のプラグインフォルダと選択するフォルダで原文/翻訳の比較を行います. 実行しますか？", null, "実行");
        if (!isConfirmed)
        {
            return;
        }

        var path = await this.CustomDialog.OpenFolderAsync();
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        _ = Task.Run(async () =>
        {
            await Task.WhenAll(
            [
                this.LanguageRepository.ReportDiffAsync(path),
                this.TranslationRepository.ReportDiffAsync(path)
            ]).ConfigureAwait(false);
            this.Snackbar.Add("プラグインフォルダにレポートを出力しました", Severity.Info);
        });
    });

    private void UpdateNotifiedLanguages()
    {
        this.NotifiedLanguages.Clear();
        foreach (var kn in this.KeyNotification.KeyNotifications)
        {
            if (this.SelectedNotificationFilter == LanguageKeyTypes.Language
                && kn.KeyType != LanguageKeyTypes.Language)
            {
                continue;
            }
            else if (this.SelectedNotificationFilter == LanguageKeyTypes.LanguageTalk
                && kn.KeyType != LanguageKeyTypes.LanguageTalk)
            {
                continue;
            }
            else if (this.IsFilterTranslated
                && this.TranslationRepository.TryGetTranslation(kn, out _))
            {
                continue;
            }
            var isIgnored = this.IgnoreList.IsIgnored(kn);
            if (this.IsFilterIgnored && isIgnored)
            {
                continue;
            }

            if (this.LanguageRepository.TryGetContainer(kn, out var lc))
            {
                this.NotifiedLanguages.Push(
                    new LanguageContainerViewModel(
                        lc, isIgnored, this.UpdateSelectedTranslationWork, x =>
                        {
                            if (x.IsIgnored)
                            {
                                this.IgnoreList.Remove(x.KeyNotification);
                                x.IsIgnored = false;
                            }
                            else
                            {
                                this.IgnoreList.Add(x.KeyNotification);
                                this.KeyNotification.Remove(x.KeyNotification);
                            }
                        }, y =>
                        {
                            this.KeyNotification.Remove(y.KeyNotification);
                        }));
            }
        }
        this.InvokeAsync(this.StateHasChanged);
    }

    private void UpdateSelectedTranslationWork(LanguageContainer lc)
    {
        var userTranslation = string.Empty;
        if (this.TranslationRepository.TryGetTranslation(lc.KeyNotification, out var tmpUserTrans))
        {
            userTranslation = tmpUserTrans;
        }

        this.SelectedTranslationWork = new TranslationWorkViewModel(lc, userTranslation, x =>
        {
            if (string.IsNullOrEmpty(x.UserTranslation))
            {
                return;
            }

            var kn = x.KeyNotification;
            this.KeyNotification.Remove(kn);
            this.TranslationRepository.SaveTranslation(kn, x.UserTranslation);
        });
        Task.Run(async () =>
        {
            if (!this.IsTranslationApiUsed)
            {
                return;
            }
            this.ApiTranslatedString = await this.TranslationApi.TranslateAsync(this.SelectedTranslationWork.SimpleChinese, default).ConfigureAwait(false);
            await this.InvokeAsync(this.StateHasChanged);
        });
        this.SelectedTranslationKeyNotification = this.SelectedTranslationWork.KeyNotification;
    }

    private void SetStory(StoryContainer sc)
    {
        this._selectedStory = sc;
        this.SelectedStoryIndex = Math.Max(0, Math.Min(this.SelectedStoryIndex, sc.Conversations.Count - 1));
        var cc = sc.Conversations[this.SelectedStoryIndex];
        this.SetConversation(cc);
    }

    private void SetStoryIndexByOffset(int idxOffset)
    {
        var tmpIdx = this.SelectedStoryIndex + idxOffset;
        if (this._selectedStory == null || tmpIdx < 0 || this._selectedStory.Conversations.Count <= tmpIdx)
        {
            return;
        }
        this.SelectedStoryIndex = tmpIdx;
        var cc = this._selectedStory.Conversations[this.SelectedStoryIndex];
        this.SetConversation(cc);
    }

    private void SetConversation(ConversationContainer cc)
    {
        if (!this.LanguageRepository.TryGetContainer(new KeyNotification
        {
            KeyType = LanguageKeyTypes.LanguageTalk,
            Key = cc.SentenceKey
        }, out var lc))
        {
            return;
        }
        this.UpdateSelectedTranslationWork(lc);
        if (0 < cc.SpeakerKey && this.LanguageRepository.TryGetContainer(new KeyNotification
        {
            KeyType = LanguageKeyTypes.LanguageTalk,
            Key = cc.SpeakerKey
        }, out var lcSpeaker))
        {
            this.SpeakerName = lcSpeaker.SimpleChinese;
        }
        else
        {
            this.SpeakerName = string.Empty;
        }
        this.InvokeAsync(this.StateHasChanged);
    }

    public void Dispose()
    {
        this.IgnoreList.Save();
        this.KeyNotification.KeyNotifications.CollectionChanged -= this.KeyNotifications_CollectionChanged;
        GC.SuppressFinalize(this);
    }
}
