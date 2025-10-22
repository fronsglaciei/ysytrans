namespace FG.Utils.YSYard.Translations.Models;

public class AppConfig
{
    public string PluginMainDirPath
    {
        get; set;
    } = string.Empty;

    public string PluginDevDirPath
    {
        get; set;
    } = string.Empty;

    public int TemporarySaveInterval
    {
        get; set;
    } = 10000;

    public string IgnoreListPath { get; set; } = ".config/ignore.json";

	public Dictionary<string, string> StaticWordTranslations { get; set; } = [];

	public string TranslationApiUrl { get; set; } = string.Empty;
}
