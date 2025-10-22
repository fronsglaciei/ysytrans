using System.Collections.Generic;

namespace FG.Defs.YSYard.Translations.Devs
{
    public class StagingLanguage
    {
        public Dictionary<int, StagingLanguageContainer> Languages
        {
            get; set;
        } = new Dictionary<int, StagingLanguageContainer>();

        public Dictionary<int, StagingLanguageContainer> LanguageTalks
        {
            get; set;
        } = new Dictionary<int, StagingLanguageContainer>();
    }
}
