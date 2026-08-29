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

    private static readonly DataContractJsonSerializerSettings _jSettings = new()
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
        foreach (var x in hm.bgvs.dzj().xml)
        {

            //obj.Languages[x.Key] = new StagingLanguageContainer
            //{
            //    Key = x.Key,
            //    Original = string.IsNullOrEmpty(x.Chinese) ? string.Empty : x.Chinese,
            //    English = string.IsNullOrEmpty(x.LanguageEng) ? string.Empty : x.LanguageEng,
            //    Placeholder = string.IsNullOrEmpty(x.LanguageJpn) ? string.Empty : x.LanguageJpn,
            //};
            obj.Languages[x.xme] = new()
            {
                Key = x.xme,
                Original = string.IsNullOrEmpty(x.xmf) ? string.Empty : x.xmf,
                English = string.IsNullOrEmpty(x.xmh) ? string.Empty : x.xmh,
                Placeholder = string.IsNullOrEmpty(x.xmi) ? string.Empty : x.xmi
            };
        }
        //foreach (var x in LanguageTalkManager.Instance.GetAllItem().Items)
        foreach (var x in hn.bgvt.dzl().xmu)
        {
            //obj.LanguageTalks[x.Key] = new StagingLanguageContainer
            //{
            //    Key = x.Key,
            //    Original = string.IsNullOrEmpty(x.Chinese) ? string.Empty : x.Chinese,
            //    English = string.IsNullOrEmpty(x.LanguageEng) ? string.Empty : x.LanguageEng,
            //    Placeholder = string.IsNullOrEmpty(x.LanguageJP) ? string.Empty : x.LanguageJP,
            //};
            obj.LanguageTalks[x.xmm] = new()
            {
                Key = x.xmm,
                Original = string.IsNullOrEmpty(x.xmn) ? string.Empty : x.xmn,
                English = string.IsNullOrEmpty(x.xmq) ? string.Empty : x.xmq,
                Placeholder = string.IsNullOrEmpty(x.xmr) ? string.Empty : x.xmr
            };
        }
        using var fs = new FileStream(PathProvider.PathDef.StagingJsonPath, FileMode.Create);
        stgSerializer.WriteObject(fs, obj);
    }

    public static void ExportStoryPlots()
    {
        //var storyMap = ResourcesManager.Instance.Load<StoryMapData>(STORY_MAP_PATH);
        var storyMap = dg.bgsl.Load<StoryMapData>(STORY_MAP_PATH);
        if (storyMap == null)
        {
            return;
        }

        var sd = new StoryDictionary();
        foreach (var m in storyMap.maps)
        {
            //var levelDataBytes = ResourcesManager.Instance.Load<TextAsset>(m.path);
            var levelDataBytes = dg.bgsl.Load<TextAsset>(m.path);
            if (levelDataBytes == null || levelDataBytes.bytes.Length < 1)
            {
                continue;
            }

            //var levelData = UtilitySpace.Utility.Deserialize2Proto<LevelData>(levelDataBytes.bytes);
            var levelData = UtilitySpace.bhi.lrx<bjs>(levelDataBytes.bytes);
            if (levelData == null)
            {
                continue;
            }

            //var t = LevelDesignerUtility.LoadTaskSource(levelData.EntrySource).TryCast<EntryTask>();
            var t = bmp.ngv(levelData.bdnk).TryCast<EntryTask>();
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
                var tt = child.nhs();
                var stp = new StoryTalkPair();
                switch (tt)
                {
                    case blr.Say:
                        var s = child.TryCast<Say>();
                        if (s != null)
                        {
                            //var conv = ConversationManager.Instance.GetItem(s.Data.conversationID);
                            var conv = gr.bgux.GetItem(s.bhlf.bdry);
                            if (conv != null)
                            {
                                //stp.SpeakerKey = conv.CharacterID;
                                stp.SpeakerKey = conv.xer;
                            }
                            //stp.SentenceKey = s.Data.conversationID;
                            stp.SentenceKey = s.bhlf.bdry;
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
                            stp.SentenceKey = spm.bhlf.bdut.bdpf;
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
                if (pt.nhs() == blr.OptionalPrivateMessage)
                {
                    var opm = pt.TryCast<OptionalPrivateMessage>();
                    if (opm != null)
                    {
                        foreach (var x in opm.messages)
                        {
                            list.Add(new()
                            {
                                //SentenceKey = x.languageTalk
                                SentenceKey = x.bdpf
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