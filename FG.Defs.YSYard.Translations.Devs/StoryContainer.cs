﻿using System.Collections.Generic;

namespace FG.Defs.YSYard.Translations.Devs
{
    public class StoryContainer
    {
        public int Id { get; set; } = -1;

        public List<ConversationContainer> Conversations { get; set; } = new List<ConversationContainer>();
    }
}
