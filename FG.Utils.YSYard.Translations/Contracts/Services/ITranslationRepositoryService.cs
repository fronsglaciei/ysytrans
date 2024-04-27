using FG.Defs.YSYard.Translations;

namespace FG.Utils.YSYard.Translations.Contracts.Services;

public interface ITranslationRepositoryService
{
    void SetPluginFolderPath(string pluginFolderPath);

    bool TryGetTranslation(KeyNotification kn, out string translation);

    void SaveTranslation(KeyNotification kn, string translation);

    Task<IEnumerable<KeyNotification>> SearchTranslationAsync(string pattern);

    Task<bool> SaveBackupAsync(string path);

    Task<bool> LoadBackupAsync(string path);

    Task<bool> SerializeAsync();
}
