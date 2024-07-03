using FG.Defs.YSYard.Translations;
using FG.Utils.YSYard.Translations.Models;
using System.Diagnostics.CodeAnalysis;

namespace FG.Utils.YSYard.Translations.Contracts.Services;

public interface ILanguageRepositoryService
{
	void SetPluginFolderPath(string pluginFolderPath);

	bool TryGetContainer(KeyNotification kn, [MaybeNullWhen(false)] out LanguageContainer container);

	bool TryGetNextContainer(KeyNotification kn, int indexOffset, [MaybeNullWhen(false)] out LanguageContainer container);

	Task<IEnumerable<KeyNotification>> SearchContainersAsync(string pattern);

    Task ReportDiffAsync(string oldPluginFolderPath);
}
