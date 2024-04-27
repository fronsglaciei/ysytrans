using FG.Defs.YSYard.Translations;
using FG.Utils.YSYard.Translations.Models;

namespace FG.Utils.YSYard.Translations.ViewModels;

public class LanguageContainerViewModel(
	LanguageContainer lc,
    bool isIgnored,
	Action<LanguageContainer> onClick,
    Action<LanguageContainerViewModel> onToggleIgnored,
	Action<LanguageContainerViewModel> onRemove)
{
    private readonly LanguageContainer _lc = lc;

    public bool IsIgnored
    {
        get; set;
    } = isIgnored;

    private readonly Action<LanguageContainer> _onClick = onClick;

    private readonly Action<LanguageContainerViewModel> _onToggleIgnored = onToggleIgnored;

    private readonly Action<LanguageContainerViewModel> _onRemove = onRemove;

    public LanguageKeyTypes KeyType => this._lc.KeyType;

    public int Key => this._lc.Key;

    public KeyNotification KeyNotification => this._lc.KeyNotification;

    public string Description => this._lc.SimpleChinese;

    public bool IsInvalid => this.KeyType == LanguageKeyTypes.Undefined || this.Key < 0;

	public void OnClick() => this._onClick(this._lc);

    public void OnToggleIgnored() => this._onToggleIgnored(this);

    public void OnRemove() => this._onRemove(this);
}
