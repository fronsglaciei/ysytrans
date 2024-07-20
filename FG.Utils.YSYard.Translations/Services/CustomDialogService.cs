using FG.Utils.YSYard.Translations.Components;
using FG.Utils.YSYard.Translations.Contracts.Services;
using FG.Utils.YSYard.Translations.Models;
using MudBlazor;

namespace FG.Utils.YSYard.Translations.Services;

public class CustomDialogService : ICustomDialogService
{
    public IDialogService? MudDialog { get; set; }

    public Func<FilePickerOptions?, Task<string>>? OpenFilePicker { get; set; }

    public Func<FilePickerOptions?, Task<string>>? SaveFilePicker { get; set; }

    public Func<Task<string>>? FolderPicker { get; set; }

    public async Task<bool> ConfirmAsync(string content, string? title = null, string? buttonText = null)
    {
        if (this.MudDialog == null)
        {
            throw new NullReferenceException($"{nameof(MudDialog)}");
        }
        var dialogParams = new DialogParameters<ConfirmDialog>
        {
            { x => x.ContentText, content },
            { x => x.ButtonText, buttonText ?? "確定" }
        };
        var dialogRef = this.MudDialog.Show<ConfirmDialog>(title ?? "確認", dialogParams);
        var res = await dialogRef.Result;
        if (res == null)
        {
            return false;
        }
        if (res.Canceled)
        {
            return false;
        }
        if (res.Data is not bool isConfirmed)
        {
            return false;
        }
        return isConfirmed;
    }

    public Task<string> OpenFileAsync(FilePickerOptions? options)
    {
        if (this.OpenFilePicker == null)
        {
            throw new NullReferenceException($"{nameof(OpenFilePicker)}");
        }
        return this.OpenFilePicker(options);
    }

    public Task<string> SaveFileAsync(FilePickerOptions? options)
    {
        if (this.SaveFilePicker == null)
        {
            throw new NullReferenceException($"{nameof(SaveFilePicker)}");
        }
        return this.SaveFilePicker(options);
    }

    public Task<string> OpenFolderAsync()
    {
        if (this.FolderPicker == null)
        {
            throw new NullReferenceException($"{nameof(FolderPicker)}");
        }
        return this.FolderPicker();
    }
}
