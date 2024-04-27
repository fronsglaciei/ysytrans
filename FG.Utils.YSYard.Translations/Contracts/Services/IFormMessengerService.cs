using FG.Utils.YSYard.Translations.Enums;
using FG.Utils.YSYard.Translations.Models;
using Microsoft.JSInterop;

namespace FG.Utils.YSYard.Translations.Contracts.Services;

public interface IFormMessengerService
{
    (FormMessengerMessageTypes msgType, int id) ParseMessage(string message);

    void RequestFolderPath(IJSRuntime jsRuntime, Action<string> onFolderPicked);

    void RespondFolderPath(int id, string path);

	void RequestSaveFilePath(IJSRuntime jsRuntime, Action<string> onFilePicked, FilePickerOptions? options = null);

	FilePickerOptions? GetSaveFilePickerOptions(int id);

	void RespondSaveFilePath(int id, string path);

	void RequestOpenFilePath(IJSRuntime jsRuntime, Action<string> onFilePicked, FilePickerOptions? options = null);

	FilePickerOptions? GetOpenFilePickerOptions(int id);

	void RespondOpenFilePath(int id, string path);
}
