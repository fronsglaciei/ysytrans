using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Threading;
using System.Threading.Tasks;

namespace FG.Defs.YSYard.Translations.Devs
{
    public class KeyNotificationClient<T> : KeyNotificationDefs, IObservable<int>
        where T : KeyNotificationBase, new()
    {
        private readonly MemoryMappedFile _mem;

        private readonly HeaderedMemoryAccess _memAccess;

        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        private readonly Task _mainTask;

        private readonly ConcurrentQueue<T> _notifications = new ConcurrentQueue<T>();

        private readonly List<IObserver<int>> _observers = new List<IObserver<int>>();

        private readonly object _lock = new object();

        public KeyNotificationClient()
        {
            this._mem = MemoryMappedFile.CreateOrOpen(MEMORY_NAME, MEMORY_ARRAY_SIZE, MemoryMappedFileAccess.ReadWrite);
            this._memAccess = new HeaderedMemoryAccess(this._mem, MEMORY_ARRAY_SIZE);
            this._mainTask = Task.Run(async () =>
            {
                while (!this._cts.IsCancellationRequested)
                {
                    var header = this._memAccess.ReadHeader();
                    if (header.Status != MemoryArrayStatus.WaitForRead
                        || header.Count < 1)
                    {
                        await Task.Delay(50).ConfigureAwait(false);
                        continue;
                    }

                    var content = this._memAccess.ReadContent(header);
                    var data = new List<T>();
                    var pos = 0;
                    for (var i = 0; i < header.Count; i++)
                    {
                        var kn = new T();
                        pos += kn.FromBytes(content, pos);

                        kn.TimeStamp = DateTime.Now;
                        this._notifications.Enqueue(kn);
                    }
                    foreach (var observer in this._observers)
                    {
                        observer.OnNext(header.Count);
                    }

                    this._memAccess.WriteHeader(MemoryArrayHeader.ReadCompleted);
                }
            }, this._cts.Token);
        }

        public IDisposable Subscribe(IObserver<int> observer)
        {
            lock (this._lock)
            {
                this._observers.Add(observer);
            }
            return new Unsubscriber(observer, this._observers);
        }

        public IEnumerable<T> Flush()
        {
            var list = new List<T>();
            while (this._notifications.TryDequeue(out var notification))
            {
                list.Add(notification);
            }
            return list;
        }

        public override void Dispose()
        {
            this._cts.Cancel();
            this._cts.Dispose();

            foreach (var observer in this._observers)
            {
                observer.OnCompleted();
            }
            this._observers.Clear();

            this._memAccess?.Dispose();

            this._mem?.Dispose();
        }

        private class Unsubscriber : IDisposable
        {
            private readonly IObserver<int> _observer;

            private readonly List<IObserver<int>> _list;

            internal Unsubscriber(IObserver<int> observer, List<IObserver<int>> list)
            {
                this._observer = observer;
                this._list = list;
            }

            public void Dispose()
            {
                for (var i = this._list.Count - 1; -1 < i; i--)
                {
                    if (this._list[i] == this._observer)
                    {
                        this._list.RemoveAt(i);
                    }
                }
            }
        }
    }
}
