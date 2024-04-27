using FG.Defs.YSYard.Translations;
using Plot;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using UnityEngine;
using SysTask = System.Threading.Tasks;

namespace FG.Mods.YSYard.Translations.Services
{
    public static class LanguageResourceExporter
    {
		private const string STORY_MAP_PATH = "Story/storyMap";

		private static SysTask.Task _exportTask = SysTask.Task.CompletedTask;

        private static readonly FieldInfo _fChildren = typeof(ParentTask).GetField("children", BindingFlags.Instance | BindingFlags.NonPublic);

		public static void ExportLanguages()
        {
            if (!_exportTask.IsCompleted)
            {
                return;
            }

            var langSerializer = new DataContractJsonSerializer(typeof(LanguageExport));
            var taskLangs = LanguageManager.Instance.GetAllItem().Items.Select(x => SysTask.Task.Run(() =>
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
                using (var fs = new FileStream(path, FileMode.Create))
                {
                    langSerializer.WriteObject(fs, obj);
                }
            }));
            var langTalkSerializer = new DataContractJsonSerializer(typeof(LanguageTalkExport));
            var taskLangTalks = LanguageTalkManager.Instance.GetAllItem().Items.Select(x => SysTask.Task.Run(() =>
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
                using (var fs = new FileStream(path, FileMode.Create))
                {
                    langTalkSerializer.WriteObject(fs, obj);
                }
            }));
            _exportTask = SysTask.Task.WhenAll(new[] { taskLangs, taskLangTalks }.SelectMany(x => x));
        }

        public static void ExportStoryPlots()
        {
            var storyMap = ResourcesManager.Instance.Load<StoryMapData>(STORY_MAP_PATH);
            if (storyMap == null)
            {
                return;
            }

            var scs = new List<StoryContainer>();
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

				if (!(LevelDesignerUtility.LoadTaskSource(levelData.EntrySource) is EntryTask t))
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
                using (var fs = File.Create(path))
                {
                    using (var writer = JsonReaderWriterFactory.CreateJsonWriter(fs, Encoding.UTF8, true, true, "  "))
                    {
                        scSerializer.WriteObject(writer, x);
                        writer.Flush();
                    }
                }
            }));
            SysTask.Task.WhenAll(tasks);
        }

        private static List<ConversationContainer> TraverseConversations(ParentTask rootNode)
        {
            var list = new List<ConversationContainer>();

            var tmpChildren = _fChildren.GetValue(rootNode);
            if (tmpChildren == null)
            {
                return list;
            }
			if (!(tmpChildren is List<Plot.Task> children))
			{
				return list;
			}

			foreach (var child in children)
            {
				if (!(child is ParentTask pt))
				{
					var tt = child.GetTaskType();
					var cc = new ConversationContainer();
					switch (tt)
					{
						case TaskType.Say:
							if (child is Say s)
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
							if (child is PrivateMessage pm)
							{
								cc.SentenceKey = pm.leftmessageTalkID;
							}
							break;

						case TaskType.SetPrivateMessage:
							if (child is SetPrivateMessage spm)
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
						if (pt is OptionalPrivateMessage opm)
						{
							list.AddRange(opm.messages.Select(x => new ConversationContainer
							{
								SentenceKey = x.languageTalk
							}));
						}
					}
					list.AddRange(TraverseConversations(pt));
				}
			}

            return list;
        }
    }
}
