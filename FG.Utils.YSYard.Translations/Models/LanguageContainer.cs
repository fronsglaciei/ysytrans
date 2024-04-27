using FG.Defs.YSYard.Translations;

namespace FG.Utils.YSYard.Translations.Models;

public class LanguageContainer : IEquatable<LanguageContainer>
{
	public static LanguageContainer Empty
	{
		get;
	} = new();

	public LanguageKeyTypes KeyType => this.KeyNotification.KeyType;

	public int Key => this.KeyNotification.Key;

	public KeyNotification KeyNotification
	{
		get; set;
	} = new KeyNotification
	{
		Key = -1
	};

	public string SimpleChinese
	{
		get; set;
	} = string.Empty;

	public string TraditionalChinese
	{
		get; set;
	} = string.Empty;

	public string English
	{
		get; set;
	} = string.Empty;

	public string Japanese
	{
		get; set;
	} = string.Empty;

    public bool Equals(LanguageContainer? other)
    {
        if (other == null)
		{
			return false;
		}
		return this.KeyType == other.KeyType && this.Key == other.Key;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not LanguageContainer lc)
		{
			return false;
		}
		return this.KeyType == lc.KeyType && this.Key == lc.Key;
    }

    public override int GetHashCode()
		=> HashCode.Combine(this.KeyType, this.Key);
}
