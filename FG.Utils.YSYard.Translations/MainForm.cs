using FG.Utils.YSYard.Translations.Interops;
using FG.Utils.YSYard.Translations.Models;
using FG.Utils.YSYard.Translations.Services;
using Microsoft.AspNetCore.Components.WebView.WindowsForms;

namespace FG.Utils.YSYard.Translations;

public partial class MainForm : Form
{
    private readonly HostWrapper _host = new();

    private readonly SynchronizationContext _thContext;

    public MainForm()
    {
        if (SynchronizationContext.Current == null)
        {
            throw new ApplicationException($"{nameof(MainForm)}");
        }
        this._thContext = SynchronizationContext.Current;

        var fDialog = this._host.GetService<CustomDialogService>();
        if (fDialog != null)
        {
            fDialog.OpenFilePicker = this.PickOpenFileAsync;
            fDialog.SaveFilePicker = this.PickSaveFileAsync;
            fDialog.FolderPicker = this.PickFolderAsync;
        }

        InitializeComponent();

        NativeMethods.UseImmersiveDarkMode(this.Handle, true);

        this.MainWebView.HostPage = "wwwroot/index.html";
        this.MainWebView.Services = this._host.Services;
        this.MainWebView.StartPath = "/";

        this.MainWebView.RootComponents.Add<Main>("#app");
    }

    private async Task<string> PickOpenFileAsync(FilePickerOptions? opts)
    {
        var tcs = new TaskCompletionSource<string>();
        this._thContext.Post(_ =>
        {
            if (opts != null)
            {
                this.MainOpenFilePicker.Filter = opts.FilterString;
            }
            if (this.MainOpenFilePicker.ShowDialog() == DialogResult.OK)
            {
                tcs.SetResult(this.MainOpenFilePicker.FileName);
            }
            else
            {
                tcs.SetResult(string.Empty);
            }
        }, null);
        return await tcs.Task;
    }

    private async Task<string> PickSaveFileAsync(FilePickerOptions? opts)
    {
        var tcs = new TaskCompletionSource<string>();
        this._thContext.Post(_ =>
        {
            if (opts != null)
            {
                this.MainSaveFilePicker.Filter = opts.FilterString;
            }
            if (this.MainSaveFilePicker.ShowDialog() == DialogResult.OK)
            {
                tcs.SetResult(this.MainSaveFilePicker.FileName);
            }
            else
            {
                tcs.SetResult(string.Empty);
            }
        }, null);
        return await tcs.Task;
    }

    private async Task<string> PickFolderAsync()
    {
        var tcs = new TaskCompletionSource<string>();
        this._thContext.Post(_ =>
        {
            if (this.MainFolderPicker.ShowDialog() == DialogResult.OK)
            {
                tcs.SetResult(this.MainFolderPicker.SelectedPath);
            }
            else
            {
                tcs.SetResult(string.Empty);
            }
        }, null);
        return await tcs.Task;
    }
}
