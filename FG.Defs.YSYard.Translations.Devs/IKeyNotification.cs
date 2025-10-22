using System;

namespace FG.Defs.YSYard.Translations.Devs
{
    public interface IKeyNotification<T> : IEquatable<T>
    {
        DateTime TimeStamp
        {
            get; set;
        }

        byte[] ToBytes();

        int FromBytes(byte[] bytes, int pos);
    }
}
