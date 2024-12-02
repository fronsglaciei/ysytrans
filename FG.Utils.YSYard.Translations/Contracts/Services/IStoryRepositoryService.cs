using FG.Defs.YSYard.Translations.Devs;
using System.Diagnostics.CodeAnalysis;

namespace FG.Utils.YSYard.Translations.Contracts.Services;

public interface IStoryRepositoryService
{
    void SetPluginFolderPath(string pluginFolderPath);

    bool TryGetStory(int id, [MaybeNullWhen(false)] out StoryContainer story);

    bool TryGetNextStory(int id, int indexOffset, [MaybeNullWhen(false)] out StoryContainer story);

    Task<StoryContainer?> SearchStoryAsync(int languageTalkKey);
}
