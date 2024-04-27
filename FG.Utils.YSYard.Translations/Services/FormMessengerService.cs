using FG.Utils.YSYard.Translations.Contracts.Services;
using FG.Utils.YSYard.Translations.Enums;
using FG.Utils.YSYard.Translations.Models;
using Microsoft.JSInterop;
using System.Collections.Concurrent;

namespace FG.Utils.YSYard.Translations.Services;

public class FormMessengerService : IFormMessengerService
{
    private const string UNIQUE_MESSAGE_TAG = "YSYTranslation";

    private const string JS_SEND_MESSAGE = "window.chrome.webview.postMessage";

    private static readonly (FormMessengerMessageTypes, int) InvalidMessage = (FormMessengerMessageTypes.Undefined, -1);

    private int _onFolderPickedLastId = 0;
    private readonly ConcurrentDictionary<int, Action<string>> _onFolderPickedCallbacks = new();

	private int _onSaveFilePickedLastId = 0;
	private readonly ConcurrentDictionary<int, Action<string>> _onSaveFilePickedCallbacks = new();
	private readonly ConcurrentDictionary<int, FilePickerOptions> _saveFilePickerOptions = new();

    private int _onOpenFilePickedLastId = 0;
    private readonly ConcurrentDictionary<int, Action<string>> _onOpenFilePickedCallbacks = new();
    private readonly ConcurrentDictionary<int, FilePickerOptions> _openFilePickerOptions = new();

	public (FormMessengerMessageTypes msgType, int id) ParseMessage(string message)
    {
        var arrParams = message.Split('-');
        if (arrParams.Length < 3)
        {
            return InvalidMessage;
        }
        if (arrParams[0] != UNIQUE_MESSAGE_TAG)
        {
            return InvalidMessage;
        }

        if (!int.TryParse(arrParams[1], out var tmpMsgType)
            || !Enum.IsDefined(typeof(FormMessengerMessageTypes), tmpMsgType))
        {
            return InvalidMessage;
        }
        var messageType = (FormMessengerMessageTypes)tmpMsgType;

        if (!int.TryParse(arrParams[2], out var tmpId))
        {
            return InvalidMessage;
        }

        return (messageType, tmpId);
    }

    public void RequestFolderPath(IJSRuntime jsRuntime, Action<string> onFolderPicked)
    {
        this._onFolderPickedCallbacks.TryAdd(this._onFolderPickedLastId, onFolderPicked);
        var capturedId = this._onFolderPickedLastId;
        Task.Run(async () =>
        {
            var strParam = CreateMessage(FormMessengerMessageTypes.PickFolder, capturedId);
            await jsRuntime.InvokeVoidAsync(JS_SEND_MESSAGE, strParam).ConfigureAwait(false);
        });
        Interlocked.Increment(ref this._onFolderPickedLastId);
    }

    public void RespondFolderPath(int id, string path)
    {
        if (this._onFolderPickedCallbacks.TryGetValue(id, out var callback))
        {
            callback(path);
        }
    }

	public void RequestSaveFilePath(IJSRuntime jsRuntime, Action<string> onFilePicked, FilePickerOptions? options = null)
	{
		this._onSaveFilePickedCallbacks.TryAdd(this._onSaveFilePickedLastId, onFilePicked);
		if (options != null)
		{
			this._saveFilePickerOptions.TryAdd(this._onSaveFilePickedLastId, options);
		}
		var capturedId = this._onSaveFilePickedLastId;
		Task.Run(async () =>
		{
			var strParam = CreateMessage(FormMessengerMessageTypes.PickSaveFile, capturedId);
			await jsRuntime.InvokeVoidAsync(JS_SEND_MESSAGE, strParam).ConfigureAwait(false);
		});
        Interlocked.Increment(ref this._onSaveFilePickedLastId);
	}

	public FilePickerOptions? GetSaveFilePickerOptions(int id)
        => this._saveFilePickerOptions.TryGetValue(id, out var result) ? result : null;

	public void RespondSaveFilePath(int id, string path)
	{
		if (this._onSaveFilePickedCallbacks.TryGetValue(id, out var callback))
		{
			callback(path);
			this._onSaveFilePickedCallbacks.TryRemove(id, out _);
			this._saveFilePickerOptions.TryRemove(id, out _);
		}
	}

	public void RequestOpenFilePath(IJSRuntime jsRuntime, Action<string> onFilePicked, FilePickerOptions? options = null)
	{
		this._onOpenFilePickedCallbacks.TryAdd(this._onOpenFilePickedLastId, onFilePicked);
        if (options != null)
        {
            this._openFilePickerOptions.TryAdd(this._onOpenFilePickedLastId, options);
        }
        var capturedId = this._onOpenFilePickedLastId;
        Task.Run(async () =>
        {
            var strParam = CreateMessage(FormMessengerMessageTypes.PickOpenFile, capturedId);
            await jsRuntime.InvokeVoidAsync(JS_SEND_MESSAGE, strParam).ConfigureAwait(false);
        });
        Interlocked.Increment(ref this._onOpenFilePickedLastId);
	}

	public FilePickerOptions? GetOpenFilePickerOptions(int id)
        => this._openFilePickerOptions.TryGetValue(id, out var result) ? result : null;

	public void RespondOpenFilePath(int id, string path)
	{
		if (this._onOpenFilePickedCallbacks.TryGetValue(id, out var callback))
        {
            callback(path);
            this._onOpenFilePickedCallbacks.TryRemove(id, out _);
            this._openFilePickerOptions.TryRemove(id, out _);
        }
	}

	private static string CreateMessage(FormMessengerMessageTypes messageType, int id)
        => $"{UNIQUE_MESSAGE_TAG}-{(int)messageType}-{id}";
}
