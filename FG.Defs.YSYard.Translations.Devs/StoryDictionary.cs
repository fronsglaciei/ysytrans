using System.Collections.Generic;

namespace FG.Defs.YSYard.Translations.Devs
{
    public class StoryDictionary
    {
        public Dictionary<string, List<StoryTalkPair>> Dict
        {
            get; set;
        } = new Dictionary<string, List<StoryTalkPair>>();
    }
}
