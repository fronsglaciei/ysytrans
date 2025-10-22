using System;

namespace FG.Defs.YSYard.Translations.Devs
{
    public class LanguageKey : KeyNotificationBase
    {
        public LanguageKeyTypes KeyType { get; set; }

        public int Key { get; set; }

        public override bool Equals(KeyNotificationBase other)
        {
            if (other == null)
            {
                return false;
            }
            if (!(other is LanguageKey tmpOther))
            {
                return false;
            }
            return this.KeyType == tmpOther.KeyType
                && this.Key == tmpOther.Key;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof(LanguageKey))
            {
                return false;
            }
            var other = (LanguageKey)obj;
            return this.KeyType == other.KeyType
                && this.Key == other.Key;
        }

        public override int GetHashCode()
            => (this.KeyType, this.Key).GetHashCode();

        public override byte[] ToBytes()
        {
            var ret = new byte[8];
            BitConverter.GetBytes((int)this.KeyType).CopyTo(ret, 0);
            BitConverter.GetBytes(this.Key).CopyTo(ret, 4);
            return ret;
        }

        public override int FromBytes(byte[] bytes, int pos)
        {
            var curPos = pos;

            var tmpKeyType = BitConverter.ToInt32(bytes, curPos);
            curPos += 4;
            try
            {
                this.KeyType = (LanguageKeyTypes)tmpKeyType;
            }
            catch { }

            this.Key = BitConverter.ToInt32(bytes, curPos);
            curPos += 4;

            return curPos - pos;
        }
    }
}
