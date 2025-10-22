using Example;
using FG.Defs.YSYard.Translations;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace FG.Mods.YSYard.Translations.Services;

public static class TranslationProvider
{
    public const int KEY_EXPLORATION_LEVEL_MAX = -10000000;

    public const int KEY_MINING_LEVEL_MAX = -10000001;

    private static readonly Dictionary<int, Language> _extraLangCache = new()
    {
        [KEY_EXPLORATION_LEVEL_MAX] = new()
        {
            Key = KEY_EXPLORATION_LEVEL_MAX,
            Chinese = "探险已经达到最高等级",
            ChineseFT = "探險已經達到最高等級",
            LanguageEng = "Reached Lv.MAX",
            LanguageJpn = "最高レベルに到達"
        },
        [KEY_MINING_LEVEL_MAX] = new()
        {
            Key = KEY_MINING_LEVEL_MAX,
            Chinese = "探险已经达到最高等级",
            ChineseFT = "探險已經達到最高等級",
            LanguageEng = "Reached Lv.MAX",
            LanguageJpn = "最高レベルに到達"
        }
    };

    private static readonly DataContractJsonSerializerSettings _serializerSettings = new()
    {
        UseSimpleDictionaryFormat = true,
    };

    private static TlData _tlData = new();

    public static void LoadTranslations()
    {
        if (!PathProvider.PathDef.IsValid)
        {
            return;
        }

        if (!File.Exists(PathProvider.PathDef.TlDataSerializedPath))
        {
            Plugin.Log.LogError($"Missed file: {PathProvider.PathDef.TlDataSerializedPath}");
            return;
        }

        var serializer = new DataContractJsonSerializer(typeof(TlData), _serializerSettings);
        var json = File.ReadAllText(PathProvider.PathDef.TlDataSerializedPath);
        using var ms = new MemoryStream(Encoding.Default.GetBytes(json));
        if (serializer.ReadObject(ms) is not TlData obj)
        {
            Plugin.Log.LogError($"Deserialization failed: {PathProvider.PathDef.TlDataSerializedPath}");
            return;
        }
        _tlData = obj;
    }

    public static bool TryGetLanguageTranslation(int key, out string languageTranslation)
        => _tlData.Languages.TryGetValue(key, out languageTranslation);

    public static bool TryGetLanguageTalkTranslation(int key, out string languageTalkTranslation)
        => _tlData.LanguageTalks.TryGetValue(key, out languageTalkTranslation);

    public static void InjectExtraLanguages()
    {
        var la = LanguageManager.mItemArray;
        if (la == null || la.Keys == null || la.Items == null)
        {
            return;
        }

        foreach (var kvp in _extraLangCache)
        {
            if (LanguageManager.mKeyIndexMap.ContainsKey(kvp.Key))
            {
                continue;
            }
            la.Keys.Add(kvp.Key);
            la.Items.Add(kvp.Value);
            LanguageManager.mKeyIndexMap[kvp.Key] = la.Keys.Count - 1;
        }
    }
}
