using FG.Defs.YSYard.Translations.Devs;
using FG.Utils.YSYard.Translations.Models;
using Microsoft.AspNetCore.Components.Web;

namespace FG.Utils.YSYard.Translations.ViewModels;

public class TranslationWorkViewModel(
	LanguageContainer lc,
	string userTranslation,
	Action<TranslationWorkViewModel> onSaveRequested)
{
	private readonly Action<TranslationWorkViewModel> _onSaveRequested = onSaveRequested;

	public LanguageKeyTypes KeyType { get; } = lc.KeyType;

	public int Key { get; } = lc.Key;

	public KeyNotification KeyNotification => lc.KeyNotification;

	public string SimpleChinese { get; } = lc.SimpleChinese;

	public string TraditionalChinese { get; } = lc.TraditionalChinese;

	public string English { get; } = lc.English;

	public string Japanese { get; } = lc.Japanese;

	public string UserTranslation { get; set; } = userTranslation;

	public void OnKeyDown(KeyboardEventArgs args)
	{
		if (args.CtrlKey && args.Key == "s")
		{
			this._onSaveRequested(this);
		}
	}
}
