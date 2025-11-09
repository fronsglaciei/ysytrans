using FG.Defs.YSYard.Translations;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace FG.Mods.YSYard.Translations.Services;

public static class TranslationProvider
{
    //private static readonly Dictionary<int, Language> _extraLangCache = [];

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

    //public static void InjectExtraLanguages()
    //{
    //    //var la = LanguageManager.mItemArray;
    //    var la = hm.waj;
    //    //if (la == null || la.Keys == null || la.Items == null)
    //    if (la == null || la.xmf == null || la.xmg == null)
    //    {
    //        return;
    //    }

    //    foreach (var kvp in _extraLangCache)
    //    {
    //        //if (LanguageManager.mKeyIndexMap.ContainsKey(kvp.Key))
    //        if (hm.wak.ContainsKey(kvp.Key))
    //        {
    //            continue;
    //        }
    //        //la.Keys.Add(kvp.Key);
    //        la.xmf.Add(kvp.Key);
    //        //la.Items.Add(kvp.Value);
    //        la.xmg.Add(kvp.Value);
    //        //LanguageManager.mKeyIndexMap[kvp.Key] = la.Keys.Count - 1;
    //        hm.wak[kvp.Key] = la.xmf.Count - 1;
    //    }
    //}
}
