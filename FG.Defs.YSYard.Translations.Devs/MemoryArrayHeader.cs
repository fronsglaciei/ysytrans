namespace FG.Defs.YSYard.Translations.Devs
{
    internal class MemoryArrayHeader
    {
        internal const int HEADER_SIZE = 12;

        internal int ContentSize
        {
            get; set;
        }

        internal int Count
        {
            get; set;
        }

        internal MemoryArrayStatus Status
        {
            get; set;
        }

        internal static MemoryArrayHeader Idle => new MemoryArrayHeader
        {
            Status = MemoryArrayStatus.Idle
        };

        internal static MemoryArrayHeader Write => new MemoryArrayHeader
        {
            Status = MemoryArrayStatus.Write
        };

        internal static MemoryArrayHeader WaitForRead(int contentSize, int count) => new MemoryArrayHeader
        {
            ContentSize = contentSize,
            Count = count,
            Status = MemoryArrayStatus.WaitForRead
        };

        internal static MemoryArrayHeader ReadCompleted => new MemoryArrayHeader
        {
            Status = MemoryArrayStatus.ReadCompleted
        };

        internal Layout ToLayout() => new Layout(this.ContentSize, this.Count, (int)this.Status);

        internal struct Layout
        {
            internal int ContentSize;

            internal int Count;

            internal int Status;

            internal Layout(int contentSize, int count, int status)
            {
                this.ContentSize = contentSize;
                this.Count = count;
                this.Status = status;
            }

            internal MemoryArrayHeader ToObject()
            {
                var obj = new MemoryArrayHeader
                {
                    ContentSize = this.ContentSize,
                    Count = this.Count
                };
                try
                {
                    obj.Status = (MemoryArrayStatus)this.Status;
                }
                catch { }
                return obj;
            }
        }
    }
}
