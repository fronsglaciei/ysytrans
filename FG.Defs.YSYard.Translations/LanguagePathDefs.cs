using System.IO;

namespace FG.Defs.YSYard.Translations
{
    public class LanguagePathDefs
    {
        private const string TRANSLATED_LANGUAGES_SERIALIZED = "translatedLanguages.json";

        private const string TRANSLATED_LANGUAGE_TALKS_SERIALIZED = "translatedLanguageTalks.json";

        public string PluginRootPath
        {
            get;
        } = "nowhere";

        public string TranslatedLanguagesSerializedPath
            => Path.Combine(this.PluginRootPath, TRANSLATED_LANGUAGES_SERIALIZED);

        public string TranslatedLanguageTalksSerializedPath
            => Path.Combine(this.PluginRootPath, TRANSLATED_LANGUAGE_TALKS_SERIALIZED);

        public bool IsValid
        {
            get
            {
                if (Directory.Exists(this.PluginRootPath))
                {
                    var assemblyFileName = Path.GetFileName(typeof(LanguagePathDefs).Assembly.Location);
                    var path = Path.Combine(this.PluginRootPath, assemblyFileName);
                    if (File.Exists(path))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public LanguagePathDefs(string rootPath)
        {
            this.PluginRootPath = rootPath;
        }
    }
}
