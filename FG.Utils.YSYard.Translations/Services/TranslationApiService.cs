using FG.Utils.YSYard.Translations.Contracts.Models;
using FG.Utils.YSYard.Translations.Models;

namespace FG.Utils.YSYard.Translations.Services;

public class TranslationApiService(IWritableOptions<AppConfig> config)
{
    public async Task<string> TranslateAsync(string text, CancellationToken token = default)
    {
        var url = config.Value.TranslationApiUrl;
        if (string.IsNullOrEmpty(url))
        {
            return string.Empty;
        }

        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["text"] = text
        });
        using var client = new HttpClient();
        var res = await client.GetAsync(
            $"{url}?{await content.ReadAsStringAsync(token).ConfigureAwait(false)}", token)
            .ConfigureAwait(false);
        return await res.Content.ReadAsStringAsync(token).ConfigureAwait(false);
    }
}
