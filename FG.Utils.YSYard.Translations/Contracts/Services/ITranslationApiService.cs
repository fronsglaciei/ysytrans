namespace FG.Utils.YSYard.Translations.Contracts.Services;

public interface ITranslationApiService
{
    Task<string> TranslateAsync(string text, CancellationToken token);
}
