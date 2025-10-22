using System;
using System.IO.MemoryMappedFiles;

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

        internal byte[] ReadContent(MemoryArrayHeader header)
        {
            var data = new byte[header.ContentSize];
            this._memAccess.ReadArray(MemoryArrayHeader.HEADER_SIZE, data, 0, header.ContentSize);
            return data;
        }

        internal void WriteContent(byte[] data)
        {
            if (this._size < MemoryArrayHeader.HEADER_SIZE + data.Length)
            {
                throw new InvalidOperationException($"Specified memory size is less than content size : {this._size}");
            }

            this._memAccess.WriteArray(MemoryArrayHeader.HEADER_SIZE, data, 0, data.Length);
        }

        public void Dispose()
        {
            this._memAccess?.Dispose();
        }
    }
}
