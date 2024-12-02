namespace FG.Defs.YSYard.Translations.Devs
{
    internal class MemoryArrayHeader
    {
        internal const int HEADER_SIZE = 8;

        internal int Count { get; set; }

        internal MemoryArrayStatus Status { get; set; }

        internal static MemoryArrayHeader Idle => new MemoryArrayHeader
        {
            Status = MemoryArrayStatus.Idle
        };

        internal static MemoryArrayHeader Write => new MemoryArrayHeader
        {
            Status = MemoryArrayStatus.Write
        };

        internal static MemoryArrayHeader WaitForRead(int count) => new MemoryArrayHeader
        {
            Count = count,
            Status = MemoryArrayStatus.WaitForRead
        };

        internal static MemoryArrayHeader ReadCompleted => new MemoryArrayHeader
        {
            Status = MemoryArrayStatus.ReadCompleted
        };

        internal Layout ToLayout() => new Layout(this.Count, (int)this.Status);

        internal struct Layout
        {
            internal int Count;

            internal int Status;

            internal Layout(int count, int status)
            {
                this.Count = count;
                this.Status = status;
            }

            internal MemoryArrayHeader ToObject()
            {
                var obj = new MemoryArrayHeader
                {
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
