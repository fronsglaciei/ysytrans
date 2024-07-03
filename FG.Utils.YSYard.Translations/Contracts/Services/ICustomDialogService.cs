using FG.Utils.YSYard.Translations.Models;
using MudBlazor;

namespace FG.Utils.YSYard.Translations.Contracts.Services;

public interface ICustomDialogService
{
    IDialogService? MudDialog
    {
        get; set;
    }

    Func<FilePickerOptions?, Task<string>>? OpenFilePicker
    {
        get; set;
    }

    Func<FilePickerOptions?, Task<string>>? SaveFilePicker
    {
        get; set;
    }

    Func<Task<string>>? FolderPicker
    {
        get; set;
    }

    Task<bool> ConfirmAsync(string content, string? title = null, string? buttonText = null);

    Task<string> OpenFileAsync(FilePickerOptions? options);

    Task<string> SaveFileAsync(FilePickerOptions? options);

    Task<string> OpenFolderAsync();
}
