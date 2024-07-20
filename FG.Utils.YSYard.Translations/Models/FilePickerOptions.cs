namespace FG.Utils.YSYard.Translations.Models;

public class FilePickerOptions
{
	public Dictionary<string, List<string>> Filters { get; } = [];

	public string FilterString =>
		string.Join("|",
			this.Filters.Select(x => string.Join("|",
				[
					x.Key,
					string.Join(";", x.Value.Select(x => $"*{x}"))
				])));
}
