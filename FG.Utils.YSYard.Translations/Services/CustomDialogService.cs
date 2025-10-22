using FG.Utils.YSYard.Translations.Models;

namespace FG.Utils.YSYard.Translations.Services;

public class CustomDialogService
{
    public Func<FilePickerOptions?, Task<string>>? OpenFilePicker { get; set; }

    public Func<FilePickerOptions?, Task<string>>? SaveFilePicker { get; set; }

    public Func<Task<string>>? FolderPicker { get; set; }

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
