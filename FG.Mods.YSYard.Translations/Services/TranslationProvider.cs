using FG.Defs.YSYard.Translations;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace FG.Mods.YSYard.Translations.Services
{
    public static class TranslationProvider
    {
        private static readonly DataContractJsonSerializerSettings _serializerSettings = new DataContractJsonSerializerSettings
        {
            UseSimpleDictionaryFormat = true,
        };

        private static ConcurrentDictionary<int, string> _langTranslations = new ConcurrentDictionary<int, string>();

        private static ConcurrentDictionary<int, string> _langTalkTranslations = new ConcurrentDictionary<int, string>();

        private static Task _loadTask = Task.CompletedTask;

        public static void LoadTranslations()
        {
            if (!PathProvider.PathDef.IsValid || !_loadTask.IsCompleted)
            {
                return;
            }
            
            var serializer0 = new DataContractJsonSerializer(typeof(TranslatedLanguages), _serializerSettings);
            var json0 = File.ReadAllText(PathProvider.PathDef.TranslatedLanguagesSerializedPath);
            using (var ms = new MemoryStream(Encoding.Default.GetBytes(json0)))
            {
                var obj = serializer0.ReadObject(ms) as TranslatedLanguages;
                if (obj == null)
                {
                    ModEntry.RootLogger.LogError($"Deserialization failed: {PathProvider.PathDef.TranslatedLanguagesSerializedPath}");
                    return;
                }
                _langTranslations = new ConcurrentDictionary<int, string>(obj.Dictionary);
            }

            var serializer1 = new DataContractJsonSerializer(typeof(TranslatedLanguageTalks), _serializerSettings);
            _loadTask = Task.Run(() =>
            {
                var json1 = File.ReadAllText(PathProvider.PathDef.TranslatedLanguageTalksSerializedPath);
                using (var ms = new MemoryStream(Encoding.Default.GetBytes(json1)))
                {
                    var obj = serializer1.ReadObject(ms) as TranslatedLanguageTalks;
                    if (obj == null)
                    {
                        ModEntry.RootLogger.LogError($"Deserialization failed: {PathProvider.PathDef.TranslatedLanguageTalksSerializedPath}");
                        return;
                    }
                    _langTalkTranslations = new ConcurrentDictionary<int, string>(obj.Dictionary);
                }
                ModEntry.RootLogger.LogInfo("Translations loaded");
            });
        }

        public static bool TryGetLanguageTranslation(int key, out string languageTranslation)
            => _langTranslations.TryGetValue(key, out languageTranslation);

        public static bool TryGetLanguageTalkTranslation(int key, out string languageTalkTranslation)
            => _langTalkTranslations.TryGetValue(key, out languageTalkTranslation);
    }
}
