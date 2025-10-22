using FG.Defs.YSYard.Translations.Devs;
using FG.Utils.YSYard.Translations.Contracts.Models;
using FG.Utils.YSYard.Translations.Models;
using System.Text.Json;

namespace FG.Utils.YSYard.Translations.Services;

public class IgnoreListService
{
    private readonly IWritableOptions<AppConfig> _config;

    private readonly HashSet<LanguageKey> _ignoredKeys = [];

    public IgnoreListService(IWritableOptions<AppConfig> config)
    {
        this._config = config;
        if (!string.IsNullOrEmpty(this._config.Value.IgnoreListPath)
            && File.Exists(this._config.Value.IgnoreListPath))
        {
            try
            {
                var json = File.ReadAllText(this._config.Value.IgnoreListPath);
                var obj = JsonSerializer.Deserialize<IgnoreList>(json);
                if (obj != null)
                {
                    foreach (var item in obj.Items)
                    {
                        this._ignoredKeys.Add(item);
                    }
                }
            }
            catch { }
        }
    }

    public bool IsIgnored(LanguageKey kn) => this._ignoredKeys.Contains(kn);

    public void Add(LanguageKey kn) => this._ignoredKeys.Add(kn);

    public void Remove(LanguageKey kn) => this._ignoredKeys.Remove(kn);

    public void Save()
    {
        if (!string.IsNullOrEmpty(this._config.Value.IgnoreListPath))
        {
            var obj = new IgnoreList
            {
                Items = [.. this._ignoredKeys]
            };
            var json = JsonSerializer.Serialize(obj);
            File.WriteAllText(this._config.Value.IgnoreListPath, json);
        }
    }
}
