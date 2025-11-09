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
        //foreach (var x in LanguageManager.Instance.GetAllItem().Items)
        foreach (var x in hm.bgvk.dzi().xmg)
        {

            //obj.Languages[x.Key] = new StagingLanguageContainer
            //{
            //    Key = x.Key,
            //    Original = string.IsNullOrEmpty(x.Chinese) ? string.Empty : x.Chinese,
            //    English = string.IsNullOrEmpty(x.LanguageEng) ? string.Empty : x.LanguageEng,
            //    Placeholder = string.IsNullOrEmpty(x.LanguageJpn) ? string.Empty : x.LanguageJpn,
            //};
            obj.Languages[x.xlz] = new()
            {
                Key = x.xlz,
                Original = string.IsNullOrEmpty(x.xma) ? string.Empty : x.xma,
                English = string.IsNullOrEmpty(x.xmc) ? string.Empty : x.xmc,
                Placeholder = string.IsNullOrEmpty(x.xmd) ? string.Empty : x.xmd
            };
        }
        //foreach (var x in LanguageTalkManager.Instance.GetAllItem().Items)
        foreach (var x in hn.bgvl.dzk().xmp)
        {
            //obj.LanguageTalks[x.Key] = new StagingLanguageContainer
            //{
            //    Key = x.Key,
            //    Original = string.IsNullOrEmpty(x.Chinese) ? string.Empty : x.Chinese,
            //    English = string.IsNullOrEmpty(x.LanguageEng) ? string.Empty : x.LanguageEng,
            //    Placeholder = string.IsNullOrEmpty(x.LanguageJP) ? string.Empty : x.LanguageJP,
            //};
            obj.LanguageTalks[x.xmh] = new()
            {
                Key = x.xmh,
                Original = string.IsNullOrEmpty(x.xmi) ? string.Empty : x.xmi,
                English = string.IsNullOrEmpty(x.xml) ? string.Empty : x.xml,
                Placeholder = string.IsNullOrEmpty(x.xmm) ? string.Empty : x.xmm
            };
        }
        using var fs = new FileStream(PathProvider.PathDef.StagingJsonPath, FileMode.Create);
        stgSerializer.WriteObject(fs, obj);
    }

    public static void ExportStoryPlots()
    {
        //var storyMap = ResourcesManager.Instance.Load<StoryMapData>(STORY_MAP_PATH);
        var storyMap = dg.bgsd.Load<StoryMapData>(STORY_MAP_PATH);
        if (storyMap == null)
        {
            return;
        }

        var sd = new StoryDictionary();
        foreach (var m in storyMap.maps)
        {
            //var levelDataBytes = ResourcesManager.Instance.Load<TextAsset>(m.path);
            var levelDataBytes = dg.bgsd.Load<TextAsset>(m.path);
            if (levelDataBytes == null || levelDataBytes.bytes.Length < 1)
            {
                continue;
            }

            //var levelData = UtilitySpace.Utility.Deserialize2Proto<LevelData>(levelDataBytes.bytes);
            var levelData = UtilitySpace.bhi.lrs<bjs>(levelDataBytes.bytes);
            if (levelData == null)
            {
                continue;
            }

            //var t = LevelDesignerUtility.LoadTaskSource(levelData.EntrySource).TryCast<EntryTask>();
            var t = bmp.ngq(levelData.bdnc).TryCast<EntryTask>();
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
                //var tt = child.GetTaskType();
                var tt = child.nhn();
                var stp = new StoryTalkPair();
                switch (tt)
                {
                    case blr.Say:
                        var s = child.TryCast<Say>();
                        if (s != null)
                        {
                            //var conv = ConversationManager.Instance.GetItem(s.Data.conversationID);
                            var conv = gr.bgup.GetItem(s.bhkx.bdrq);
                            if (conv != null)
                            {
                                //stp.SpeakerKey = conv.CharacterID;
                                stp.SpeakerKey = conv.xem;
                            }
                            //stp.SentenceKey = s.Data.conversationID;
                            stp.SentenceKey = s.bhkx.bdrq;
                        }
                        break;

                    case blr.PrivateMessage:
                        var pm = child.TryCast<PrivateMessage>();
                        if (pm != null)
                        {
                            stp.SentenceKey = pm.leftmessageTalkID;
                        }
                        break;

                    case blr.SetPrivateMessage:
                        var spm = child.TryCast<SetPrivateMessage>();
                        if (spm != null)
                        {
                            //stp.SentenceKey = spm.Data.message.languageTalk;
                            stp.SentenceKey = spm.bhkx.bdul.bdox;
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
                //if (pt.GetTaskType() == TaskType.OptionalPrivateMessage)
                if (pt.nhn() == blr.OptionalPrivateMessage)
                {
                    var opm = pt.TryCast<OptionalPrivateMessage>();
                    if (opm != null)
                    {
                        foreach (var x in opm.messages)
                        {
                            list.Add(new()
                            {
                                //SentenceKey = x.languageTalk
                                SentenceKey = x.bdox
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