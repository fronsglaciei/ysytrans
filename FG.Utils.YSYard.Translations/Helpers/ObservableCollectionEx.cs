using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace FG.Utils.YSYard.Translations.Helpers;

internal class ObservableCollectionEx<T> : ObservableCollection<T>, IDisposable
{
	private readonly ReaderWriterLockSlim _lock = new();

	internal void Overwrite(IEnumerable<T> collection)
	{
		using (var wl = new WriteLock(this._lock))
		{
            this.Items.Clear();
            foreach (var item in collection)
            {
                this.Items.Add(item);
            }
        }
		this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
	}

	public void Dispose()
	{
		this._lock.Dispose();
	}
}
