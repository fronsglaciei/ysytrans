using System;

namespace FG.Defs.YSYard.Translations.Devs
{
    public class KeyNotification : IEquatable<KeyNotification>
    {
        public LanguageKeyTypes KeyType { get; set; }

        public int Key { get; set; }

        public DateTime TimeStamp { get; set; }

        public bool Equals(KeyNotification other)
        {
            if (other == null)
            {
                return false;
            }
            return this.KeyType == other.KeyType && this.Key == other.Key;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof(KeyNotification))
            {
                return false;
            }
            var other = (KeyNotification)obj;
            return this.KeyType == other.KeyType || this.Key == other.Key;
        }

        public override int GetHashCode()
            => (this.KeyType, this.Key).GetHashCode();

        internal Layout ToStruct() => new Layout((int)this.KeyType, this.Key);

        internal struct Layout
        {
            internal int KeyType;

            internal int Key;

            internal Layout(int keyType, int key)
            {
                this.KeyType = keyType;
                this.Key = key;
            }

            internal KeyNotification ToObject()
            {
                var obj = new KeyNotification
                {
                    Key = this.Key
                };
                try
                {
                    obj.KeyType = (LanguageKeyTypes)this.KeyType;
                }
                catch { }
                return obj;
            }
        }
    }
}
