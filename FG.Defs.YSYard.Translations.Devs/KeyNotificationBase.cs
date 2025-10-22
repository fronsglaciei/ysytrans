using System;

namespace FG.Defs.YSYard.Translations.Devs
{
    public abstract class KeyNotificationBase : IKeyNotification<KeyNotificationBase>
    {
        public virtual DateTime TimeStamp
        {
            get; set;
        }

        public abstract bool Equals(KeyNotificationBase other);

        public abstract byte[] ToBytes();

        public abstract int FromBytes(byte[] bytes, int pos);
    }
}
