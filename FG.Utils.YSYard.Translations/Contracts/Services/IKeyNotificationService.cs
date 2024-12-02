using FG.Defs.YSYard.Translations.Devs;
using System.Collections.ObjectModel;

namespace FG.Utils.YSYard.Translations.Contracts.Services;

public interface IKeyNotificationService
{
	ObservableCollection<KeyNotification> KeyNotifications { get; }

	void ForceNotify(IEnumerable<KeyNotification> kns);

	void Remove(KeyNotification kn);

	void ClearAll();
}
