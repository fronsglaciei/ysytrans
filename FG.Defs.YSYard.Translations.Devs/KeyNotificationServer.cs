using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FG.Defs.YSYard.Translations.Devs
{
    public class KeyNotificationServer<T> : KeyNotificationDefs
        where T : KeyNotificationBase, new()
    {
        private readonly MemoryMappedFile _mem;

        private readonly HeaderedMemoryAccess _memAccess;

        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        private readonly Task _mainTask;

        private readonly ConcurrentQueue<T> _notifications = new ConcurrentQueue<T>();

        public KeyNotificationServer()
        {
            this._mem = MemoryMappedFile.CreateOrOpen(MEMORY_NAME, MEMORY_ARRAY_SIZE, MemoryMappedFileAccess.ReadWrite);
            this._memAccess = new HeaderedMemoryAccess(this._mem, MEMORY_ARRAY_SIZE);
            this._memAccess.WriteHeader(MemoryArrayHeader.Idle);

            this._mainTask = Task.Run(async () =>
            {
                while (!this._cts.IsCancellationRequested)
                {
                    if (!this.TryWriteNotifications())
                    {
                        continue;
                    }

                    await this.WaitForReadingAsync().ConfigureAwait(false);
                }
            }, this._cts.Token);
        }

        public void Notify(T kn) => this._notifications.Enqueue(kn);

        private bool TryWriteNotifications()
        {
            if (!this._notifications.Any())
            {
                return false;
            }

            var header = this._memAccess.ReadHeader();
            if (!(header.Status == MemoryArrayStatus.Idle
                || header.Status == MemoryArrayStatus.ReadCompleted))
            {
                return false;
            }
            this._memAccess.WriteHeader(MemoryArrayHeader.Write);

            var deqCnt = Math.Min(this._notifications.Count, MAX_ELEMENTS_COUNT);
            var tmp = new List<T>();
            for (var i = 0; i < deqCnt; i++)
            {
                if (this._notifications.TryDequeue(out var notification))
                {
                    tmp.Add(notification);
                }
            }
            var content = this.MergeToBytes(tmp);
            try
            {
                this._memAccess.WriteContent(content);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }

            this._memAccess.WriteHeader(MemoryArrayHeader.WaitForRead(content.Length, tmp.Count));
            return true;
        }

        private byte[] MergeToBytes(List<T> kns)
        {
            var tmpBytes = kns.Select(x => x.ToBytes()).ToList();
            var totalSize = tmpBytes.Select(x => x.Length).Sum();
            var buf = new byte[totalSize];
            var pos = 0;
            foreach (var arr in tmpBytes)
            {
                arr.CopyTo(buf, pos);
                pos += arr.Length;
            }
            return buf;
        }

        private async Task WaitForReadingAsync()
        {
            while (!this._cts.IsCancellationRequested)
            {
                if (this._memAccess.ReadHeader().Status == MemoryArrayStatus.ReadCompleted)
                {
                    return;
                }

                await Task.Delay(100).ConfigureAwait(false);
            }
        }

        public override void Dispose()
        {
            this._cts.Cancel();
            this._cts.Dispose();

            this._memAccess?.Dispose();

            this._mem?.Dispose();
        }
    }
}
