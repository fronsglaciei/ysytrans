using FG.Utils.YSYard.Translations.Contracts.Models;
using FG.Utils.YSYard.Translations.Contracts.Services;
using FG.Utils.YSYard.Translations.Models;

namespace FG.Utils.YSYard.Translations.Services;

public class GoogleLanguageApiService(IWritableOptions<AppConfig> config) : ITranslationApiService
{
    private readonly IWritableOptions<AppConfig> _config = config;

    public async Task<string> TranslateAsync(string text, CancellationToken token)
    {
        var url = this._config.Value.TranslationApiUrl;
        if (string.IsNullOrEmpty(url))
        {
            return string.Empty;
        }

        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["text"] = text
        });
        using var client = new HttpClient();
        var res = await client.GetAsync($"{url}?{await content.ReadAsStringAsync(token).ConfigureAwait(false)}", token).ConfigureAwait(false);
        return await res.Content.ReadAsStringAsync(token).ConfigureAwait(false);
    }
}
