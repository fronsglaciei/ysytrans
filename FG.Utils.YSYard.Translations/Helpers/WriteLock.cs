namespace FG.Utils.YSYard.Translations.Helpers;

internal readonly struct WriteLock : IDisposable
{
    /// <summary>
    /// ロックオブジェクトの内部オブジェクト
    /// </summary>
    private readonly ReaderWriterLockSlim _obj;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="obj">ロックオブジェクト</param>
    internal WriteLock(ReaderWriterLockSlim obj)
    {
        this._obj = obj;
        obj.EnterWriteLock();
    }

    public void Dispose() => this._obj.ExitWriteLock();
}
