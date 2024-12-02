using FG.Defs.YSYard.Translations.Devs;
using FG.Utils.YSYard.Translations.Contracts.Services;
using FG.Utils.YSYard.Translations.Helpers;
using System.Collections.ObjectModel;

namespace FG.Utils.YSYard.Translations.Services;

public class KeyNotificationService : IKeyNotificationService, IObserver<int>, IDisposable
{
	private readonly KeyNotificationClient _client = new();

	private readonly IDisposable _unsubscriber;

	private readonly ReaderWriterLockSlim _lock = new();

	private readonly HashSet<KeyNotification> _cache = [];

	private readonly ObservableCollectionEx<KeyNotification> _notifications = [];
	public ObservableCollection<KeyNotification> KeyNotifications => this._notifications;

	public KeyNotificationService()
	{
		this._unsubscriber = this._client.Subscribe(this);
	}

    public void ForceNotify(IEnumerable<KeyNotification> kns)
    {
		if (!kns.Any())
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

    public void Remove(KeyNotification kn)
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
        this._notifications.Overwrite(tmp);
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

    public void OnError(Exception error) { }

    public void Dispose()
	{
		this._unsubscriber.Dispose();
		this._client.Dispose();
		this._notifications.Dispose();
		this._lock.Dispose();
		GC.SuppressFinalize(this);
	}
}
