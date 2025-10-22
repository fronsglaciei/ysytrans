using FG.Defs.YSYard.Translations.Devs;
using FG.Utils.YSYard.Translations.Helpers;

namespace FG.Utils.YSYard.Translations.Services;

public class KeyNotificationService : IObserver<int>, IDisposable
{
    private readonly KeyNotificationClient<LanguageKey> _client = new();

    private readonly IDisposable _unsubscriber;

    private readonly ReaderWriterLockSlim _lock = new();

    private readonly HashSet<LanguageKey> _cache = [];

    private readonly HashSet<Action<List<LanguageKey>>> _callbacks = [];

    public KeyNotificationService()
    {
        this._unsubscriber = this._client.Subscribe(this);
    }

    public void RegisterCallback(Action<List<LanguageKey>> callback)
    {
        this._callbacks.Add(callback);
    }

    public void ForceNotify(List<LanguageKey> kns)
    {
        if (kns.Count < 1)
        {
            return;
        }

        foreach (var kn in kns)
        {
            if (this._cache.TryGetValue(kn, out var tmpKn))
            {
                using var wl = new WriteLock(this._lock);
                tmpKn.TimeStamp = kn.TimeStamp;
            }
            else
            {
                using var wl = new WriteLock(this._lock);
                this._cache.Add(kn);
            }
        }
        this.UpdateNotifications();
    }

    public void ForceNotifyAll() => this.UpdateNotifications();

    public void Remove(LanguageKey kn)
    {
        this._cache.Remove(kn);
        this.UpdateNotifications();
    }

    public void ClearAll()
    {
        this._cache.Clear();
        this.UpdateNotifications();
    }

    private void UpdateNotifications()
    {
        var tmp = this._cache.ToList();
        tmp.Sort((a, b) => a.TimeStamp.CompareTo(b.TimeStamp));
        foreach (var callback in this._callbacks)
        {
            callback(tmp);
        }
    }

    public void OnNext(int _)
    {
        var data = this._client.Flush();
        foreach (var kn in data)
        {
            using var wl = new WriteLock(this._lock);
            this._cache.Add(kn);
        }
        this.UpdateNotifications();
    }

    public void OnCompleted() { }

    public void OnError(Exception _) { }

    public void Dispose()
    {
        this._unsubscriber.Dispose();
        this._client.Dispose();
        this._lock.Dispose();
        GC.SuppressFinalize(this);
    }
}
