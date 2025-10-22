using FG.Defs.YSYard.Translations.Devs;

namespace FG.Utils.YSYard.Translations.ViewModels;

public class StagingLanguageContainerViewModel(
    StagingLanguageContainer slc,
    LanguageKey kn,
    bool isIgnored)
{
    public StagingLanguageContainer Raw { get; } = slc;

    public LanguageKey NotifiedKey { get; } = kn;

    public bool IsIgnored { get; set; } = isIgnored;

    public void UpdateUserTranslation(string value)
    {
        this.Raw.Japanese = value;
        this.Raw.IsTranslated = true;
    }
}