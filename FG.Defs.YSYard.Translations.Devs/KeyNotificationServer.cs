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
    public class KeyNotificationServer : KeyNotificationDefs
    {
        private readonly MemoryMappedFile _mem;

        private readonly HeaderedMemoryAccess _memAccess;

        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        private readonly Task _mainTask;

        private readonly ConcurrentQueue<KeyNotification> _notifications = new ConcurrentQueue<KeyNotification>();

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

        public void Notify(KeyNotification kn) => this._notifications.Enqueue(kn);

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
            var tmp = new List<KeyNotification>();
            for (var i = 0; i < deqCnt; i++)
            {
                if (this._notifications.TryDequeue(out var notification))
                {
                    tmp.Add(notification);
                }
            }
            var data = tmp.Select(x => x.ToStruct()).ToArray();
            try
            {
                this._memAccess.WriteData(data);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }

            this._memAccess.WriteHeader(MemoryArrayHeader.WaitForRead(data.Length));
            return true;
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
