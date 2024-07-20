namespace FG.Utils.YSYard.Translations.Models;

public class AppConfig
{
	public string PluginFolderPath { get; set; } = string.Empty;

	public string IgnoreListPath { get; set; } = ".config/ignore.json";

	public Dictionary<string, string> StaticWordTranslations { get; set; } = [];

	public string TranslationApiUrl { get; set; } = string.Empty;
}
