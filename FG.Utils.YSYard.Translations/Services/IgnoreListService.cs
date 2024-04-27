using FG.Defs.YSYard.Translations;
using FG.Utils.YSYard.Translations.Contracts.Models;
using FG.Utils.YSYard.Translations.Contracts.Services;
using FG.Utils.YSYard.Translations.Models;
using System.Text;
using System.Text.Json;

namespace FG.Utils.YSYard.Translations.Services;

public class IgnoreListService : IIgnoreListService
{
    private readonly IWritableOptions<AppConfig> _config;

    private readonly HashSet<KeyNotification> _ignoredKeys = [];

    public IgnoreListService(IWritableOptions<AppConfig> config)
    {
        this._config = config;
        if (!string.IsNullOrEmpty(this._config.Value.IgnoreListPath)
            && File.Exists(this._config.Value.IgnoreListPath))
        {
            try
            {
                var json = File.ReadAllText(this._config.Value.IgnoreListPath, Encoding.UTF8);
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

    public bool IsIgnored(KeyNotification kn) => this._ignoredKeys.Contains(kn);

    public void Add(KeyNotification kn) => this._ignoredKeys.Add(kn);

    public void Remove(KeyNotification kn) => this._ignoredKeys.Remove(kn);

    public void Save()
    {
        if (!string.IsNullOrEmpty(this._config.Value.IgnoreListPath))
        {
            var obj = new IgnoreList
            {
                Items = new(this._ignoredKeys)
            };
            var json = JsonSerializer.Serialize(obj);
            File.WriteAllText(this._config.Value.IgnoreListPath, json, Encoding.UTF8);
        }
    }
}
