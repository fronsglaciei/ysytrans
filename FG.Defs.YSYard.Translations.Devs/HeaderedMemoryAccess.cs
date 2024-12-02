using System;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

namespace FG.Defs.YSYard.Translations.Devs
{
    internal class HeaderedMemoryAccess : IDisposable
    {
        private readonly MemoryMappedViewAccessor _memAccess;

        private readonly int _size;

        internal HeaderedMemoryAccess(MemoryMappedFile mmf, int size)
        {
            if (size < MemoryArrayHeader.HEADER_SIZE)
            {
                throw new ArgumentException($"Specified memory size is less than header size : {size}");
            }

            this._memAccess = mmf.CreateViewAccessor(0, size);
            this._size = size;
        }

        internal MemoryArrayHeader ReadHeader()
        {
            try
            {
                this._memAccess.Read<MemoryArrayHeader.Layout>(0, out var header);
                return header.ToObject();
            }
            catch
            {
                return new MemoryArrayHeader();
            }
        }

        internal void WriteHeader(MemoryArrayHeader header)
        {
            var tmp = header.ToLayout();
            this._memAccess.Write(0, ref tmp);
        }

        internal T[] ReadData<T>(int requestedCount) where T : struct
        {
            if (requestedCount < 1)
            {
                return new T[0];
            }
            var cnt = Math.Min(requestedCount, this._size / Marshal.SizeOf(typeof(T)));
            var data = new T[cnt];
            this._memAccess.ReadArray(MemoryArrayHeader.HEADER_SIZE, data, 0, cnt);
            return data;
        }

        internal void WriteData<T>(T[] data) where T : struct
        {
            if (data.Length < 1)
            {
                return;
            }
            var cnt = Math.Min(data.Length, this._size / Marshal.SizeOf(typeof(T)));
            this._memAccess.WriteArray(MemoryArrayHeader.HEADER_SIZE, data, 0, cnt);
        }

        public void Dispose()
        {
            this._memAccess?.Dispose();
        }
    }
}
