using FG.Defs.YSYard.Translations.Devs;
using Plot;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using UnityEngine;
using SysTask = System.Threading.Tasks;

namespace FG.Mods.YSYard.Translations.Devs.Services;

public static class LanguageResourceExporter
{
    private const string STORY_MAP_PATH = "Story/storyMap";

    private static readonly DataContractJsonSerializerSettings _jSettings = new DataContractJsonSerializerSettings
    {
        UseSimpleDictionaryFormat = true,
    };

    private static SysTask.Task _exportTask = SysTask.Task.CompletedTask;

    public static void ExportLanguages()
    {
        if (!_exportTask.IsCompleted)
        {
            return;
        }

        var stgSerializer = new DataContractJsonSerializer(typeof(StagingLanguage), _jSettings);
        var obj = new StagingLanguage();
        foreach (var x in LanguageManager.Instance.GetAllItem().Items)
        {
            obj.Languages[x.Key] = new StagingLanguageContainer
            {
                Key = x.Key,
                Original = string.IsNullOrEmpty(x.Chinese) ? string.Empty : x.Chinese,
                English = string.IsNullOrEmpty(x.LanguageEng) ? string.Empty : x.LanguageEng,
                Placeholder = string.IsNullOrEmpty(x.LanguageJpn) ? string.Empty : x.LanguageJpn,
            };
        }
        foreach (var x in LanguageTalkManager.Instance.GetAllItem().Items)
        {
            obj.LanguageTalks[x.Key] = new StagingLanguageContainer
            {
                Key = x.Key,
                Original = string.IsNullOrEmpty(x.Chinese) ? string.Empty : x.Chinese,
                English = string.IsNullOrEmpty(x.LanguageEng) ? string.Empty : x.LanguageEng,
                Placeholder = string.IsNullOrEmpty(x.LanguageJP) ? string.Empty : x.LanguageJP,
            };
        }
        using var fs = new FileStream(PathProvider.PathDef.StagingJsonPath, FileMode.Create);
        stgSerializer.WriteObject(fs, obj);
    }

    public static void ExportStoryPlots()
    {
        var storyMap = ResourcesManager.Instance.Load<StoryMapData>(STORY_MAP_PATH);
        if (storyMap == null)
        {
            return;
        }

        var sd = new StoryDictionary();
        foreach (var m in storyMap.maps)
        {
            var levelDataBytes = ResourcesManager.Instance.Load<TextAsset>(m.path);
            if (levelDataBytes == null || levelDataBytes.bytes.Length < 1)
            {
                continue;
            }

            var levelData = UtilitySpace.Utility.Deserialize2Proto<LevelData>(levelDataBytes.bytes);
            if (levelData == null)
            {
                continue;
            }

            var t = LevelDesignerUtility.LoadTaskSource(levelData.EntrySource).TryCast<EntryTask>();
            if (t == null)
            {
                continue;
            }

            var stp = TraverseTalkPairs(t);
            if (sd.Dict.TryGetValue(m.storyID, out var tmpPairs))
            {
                tmpPairs.AddRange(stp);
            }
            else
            {
                sd.Dict[m.storyID] = stp;
            }
        }

        var sdSerializer = new DataContractJsonSerializer(typeof(StoryDictionary), _jSettings);
        using var fs = File.Create(PathProvider.PathDef.StoryPath);
        using var writer = JsonReaderWriterFactory.CreateJsonWriter(fs, Encoding.UTF8, true, true, "  ");
        sdSerializer.WriteObject(writer, sd);
        writer.Flush();
    }

    private static List<StoryTalkPair> TraverseTalkPairs(ParentTask rootNode)
    {
        var list = new List<StoryTalkPair>();
        foreach (var child in rootNode.children)
        {
            var pt = child.TryCast<ParentTask>();
            if (pt == null)
            {
                var tt = child.GetTaskType();
                var stp = new StoryTalkPair();
                switch (tt)
                {
                    case TaskType.Say:
                        var s = child.TryCast<Say>();
                        if (s != null)
                        {
                            var conv = ConversationManager.Instance.GetItem(s.Data.conversationID);
                            if (conv != null)
                            {
                                stp.SpeakerKey = conv.CharacterID;
                            }
                            stp.SentenceKey = s.Data.conversationID;
                        }
                        break;

                    case TaskType.PrivateMessage:
                        var pm = child.TryCast<PrivateMessage>();
                        if (pm != null)
                        {
                            stp.SentenceKey = pm.leftmessageTalkID;
                        }
                        break;

                    case TaskType.SetPrivateMessage:
                        var spm = child.TryCast<SetPrivateMessage>();
                        if (spm != null)
                        {
                            stp.SentenceKey = spm.Data.message.languageTalk;
                        }
                        break;
                }
                if (-1 < stp.SpeakerKey && -1 < stp.SentenceKey)
                {
                    list.Add(stp);
                }
            }
            else
            {
                if (pt.GetTaskType() == TaskType.OptionalPrivateMessage)
                {
                    var opm = pt.TryCast<OptionalPrivateMessage>();
                    if (opm != null)
                    {
                        foreach (var x in opm.messages)
                        {
                            list.Add(new()
                            {
                                SentenceKey = x.languageTalk
                            });
                        }
                    }
                }
                list.AddRange(TraverseTalkPairs(pt));
            }
        }

        return list;
    }
}