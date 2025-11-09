using System;

namespace FG.Defs.YSYard.Translations.Devs
{
    public abstract class KeyNotificationDefs : IDisposable
    {
        protected const int MEMORY_ARRAY_SIZE = 16384;

        protected const int MAX_ELEMENTS_COUNT = 2047;

        protected const string MEMORY_NAME = "KeyNotificationMap";

        public abstract void Dispose();
    }
}
