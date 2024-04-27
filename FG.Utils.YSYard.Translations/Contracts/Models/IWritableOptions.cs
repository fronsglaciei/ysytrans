using Microsoft.Extensions.Options;

namespace FG.Utils.YSYard.Translations.Contracts.Models;

public interface IWritableOptions<out T> : IOptions<T> where T : class, new()
{
	void Update(Action<T> applyChanges);
}
