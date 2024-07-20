using FG.Defs.YSYard.Translations;
using Plot;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using UnityEngine;
using SysTask = System.Threading.Tasks;

namespace FG.Mods.YSYard.Translations.Services;

public static class LanguageResourceExporter
{
    private const string STORY_MAP_PATH = "Story/storyMap";

    private static SysTask.Task _exportTask = SysTask.Task.CompletedTask;

    public static void ExportLanguages()
    {
        if (!_exportTask.IsCompleted)
        {
            return;
        }

        var langSerializer = new DataContractJsonSerializer(typeof(LanguageExport));
        var taskLangs = new List<SysTask.Task>();
        foreach (var x in LanguageManager.Instance.GetAllItem().Items)
        {
            taskLangs.Add(SysTask.Task.Run(() =>
            {
                var obj = new LanguageExport
                {
                    Key = x.Key,
                    SimpleChinese = x.Chinese ?? string.Empty,
                    TraditionalChinese = x.ChineseFT ?? string.Empty,
                    English = x.LanguageEng ?? string.Empty,
                    Japanese = x.LanguageJpn ?? string.Empty
                };
                var path = PathProvider.PathDef.GetExportedFilePath(new KeyNotification
                {
                    KeyType = LanguageKeyTypes.Language,
                    Key = x.Key,
                });
                using var fs = new FileStream(path, FileMode.Create);
                langSerializer.WriteObject(fs, obj);
            }));
        }
        var langTalkSerializer = new DataContractJsonSerializer(typeof(LanguageTalkExport));
        var taskLangTalks = new List<SysTask.Task>();
        foreach (var x in LanguageTalkManager.Instance.GetAllItem().Items)
        {
            taskLangTalks.Add(SysTask.Task.Run(() =>
            {
                var obj = new LanguageTalkExport
                {
                    Key = x.Key,
                    SimpleChinese = x.Chinese ?? string.Empty,
                    TraditionalChinese = x.ChineseFT ?? string.Empty,
                    English = x.LanguageEng ?? string.Empty,
                    Japanese = x.LanguageJP ?? string.Empty
                };
                var path = PathProvider.PathDef.GetExportedFilePath(new KeyNotification
                {
                    KeyType = LanguageKeyTypes.LanguageTalk,
                    Key = x.Key,
                });
                using var fs = new FileStream(path, FileMode.Create);
                langTalkSerializer.WriteObject(fs, obj);
            }));
        }
        _exportTask = SysTask.Task.WhenAll(new[] { taskLangs, taskLangTalks }.SelectMany(x => x));
    }

    public static void ExportStoryPlots()
    {
        var storyMap = ResourcesManager.Instance.Load<StoryMapData>(STORY_MAP_PATH);
        if (storyMap == null)
        {
            return;
        }

        var i = 0;
        var scs = new List<StoryContainer>();
        foreach (var m in storyMap.maps)
        {
            i++;
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

            var sc = new StoryContainer
            {
                Id = int.TryParse(m.storyID, out var tmpId) ? tmpId : -1,
                Conversations = TraverseConversations(t)
            };
            if (sc.Id < 0)
            {
                continue;
            }
            scs.Add(sc);
        }
        var scSerializer = new DataContractJsonSerializer(typeof(StoryContainer));
        var tasks = scs.Select(x => SysTask.Task.Run(() =>
        {
            var path = Path.Combine(PathProvider.PathDef.StoriesPath, $"{x.Id}.json");
            using var fs = File.Create(path);
            using var writer = JsonReaderWriterFactory.CreateJsonWriter(fs, Encoding.UTF8, true, true, "  ");
            scSerializer.WriteObject(writer, x);
            writer.Flush();
        }));
        SysTask.Task.WhenAll(tasks);
    }

    private static List<ConversationContainer> TraverseConversations(ParentTask rootNode)
    {
        var list = new List<ConversationContainer>();
        foreach (var child in rootNode.children)
        {
            var pt = child.TryCast<ParentTask>();
            if (pt == null)
            {
                var tt = child.GetTaskType();
                var cc = new ConversationContainer();
                switch (tt)
                {
                    case TaskType.Say:
                        var s = child.TryCast<Say>();
                        if (s != null)
                        {
                            var conv = ConversationManager.Instance.GetItem(s.Data.conversationID);
                            if (conv != null)
                            {
                                cc.SpeakerKey = conv.CharacterID;
                            }
                            cc.SentenceKey = s.Data.conversationID;
                        }
                        break;

                    case TaskType.PrivateMessage:
                        var pm = child.TryCast<PrivateMessage>();
                        if (pm != null)
                        {
                            cc.SentenceKey = pm.leftmessageTalkID;
                        }
                        break;

                    case TaskType.SetPrivateMessage:
                        var spm = child.TryCast<SetPrivateMessage>();
                        if (spm != null)
                        {
                            cc.SentenceKey = spm.Data.message.languageTalk;
                        }
                        break;
                }
                if (-1 < cc.SpeakerKey && -1 < cc.SentenceKey)
                {
                    list.Add(cc);
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
                list.AddRange(TraverseConversations(pt));
            }
        }

        return list;
    }
}