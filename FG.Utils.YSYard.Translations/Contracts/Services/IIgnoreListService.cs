using FG.Defs.YSYard.Translations;

namespace FG.Utils.YSYard.Translations.Contracts.Services;

public interface IIgnoreListService
{
    bool IsIgnored(KeyNotification kn);

    void Add(KeyNotification kn);

    void Remove(KeyNotification kn);

    void Save();
}
