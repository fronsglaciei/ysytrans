using FG.Utils.YSYard.Translations.Contracts.Services;
using FG.Utils.YSYard.Translations.Enums;
using FG.Utils.YSYard.Translations.Interops;
using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Web.WebView2.Core;

namespace FG.Utils.YSYard.Translations;

public partial class MainForm : Form
{
	private readonly IFormMessengerService _messenger;

    public MainForm()
    {
		this._messenger = Startup.GetService<IFormMessengerService>();

		InitializeComponent();

		this.MainWebView.BlazorWebViewInitialized += (s, e) =>
		{
			e.WebView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
			e.WebView.CoreWebView2.ContextMenuRequested += this.CoreWebView2_ContextMenuRequested;
			e.WebView.CoreWebView2.WebMessageReceived += this.CoreWebView2_WebMessageReceived;
		};

		NativeMethods.UseImmersiveDarkMode(this.Handle, true);

        this.MainWebView.HostPage = "wwwroot/index.html";
        this.MainWebView.Services = Startup.Services!;
        this.MainWebView.StartPath = "/";

        this.MainWebView.RootComponents.Add<Main>("#app");
    }

	private void CoreWebView2_ContextMenuRequested(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2ContextMenuRequestedEventArgs e)
	{
		// https://github.com/dotnet/maui/issues/11398

		if (e.ContextMenuTarget.IsEditable)
		{
			var itemNamesToRemove = new[] { "share", "webSelect", "webCapture", "inspectElement" };
			var menuIndexesToRemove =
				e.MenuItems
					.Select((m, i) => (m, i))
					.Where(m => itemNamesToRemove.Contains(m.m.Name))
					.Select(m => m.i)
					.Reverse();

			foreach (var menuIndexToRemove in menuIndexesToRemove)
			{
				e.MenuItems.RemoveAt(menuIndexToRemove);
			}

			while (e.MenuItems.Last().Kind == Microsoft.Web.WebView2.Core.CoreWebView2ContextMenuItemKind.Separator)
			{
				e.MenuItems.RemoveAt(e.MenuItems.Count - 1);
			}
		}
		else
		{
			e.Handled = true;
		}
	}

	private void CoreWebView2_WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
	{
		// https://github.com/MicrosoftEdge/WebView2Feedback/issues/3028
		// https://learn.microsoft.com/en-us/microsoft-edge/webview2/concepts/threading-model

		var context = System.Threading.SynchronizationContext.Current;
		if (context == null)
		{
			return;
		}

		var msg = e.TryGetWebMessageAsString();
		var (msgType, id) = this._messenger.ParseMessage(msg);
		if (msgType == FormMessengerMessageTypes.Undefined)
		{
			return;
		}
		else if (msgType == FormMessengerMessageTypes.PickFolder)
		{
			context.Post(_ =>
			{
				var res = this.MainFolderPicker.ShowDialog();
				if (res == DialogResult.OK)
				{
					this._messenger.RespondFolderPath(id, this.MainFolderPicker.SelectedPath);
				}
			}, null);
		}
		else if (msgType == FormMessengerMessageTypes.PickSaveFile)
		{
			context.Post(_ =>
			{
				var opts = this._messenger.GetSaveFilePickerOptions(id);
				if (opts != null)
				{
					this.MainSaveFilePicker.Filter = opts.FilterString;
				}
				var res = this.MainSaveFilePicker.ShowDialog();
				if (res == DialogResult.OK)
				{
					this._messenger.RespondSaveFilePath(id, this.MainSaveFilePicker.FileName);
				}
			}, null);
		}
		else if (msgType == FormMessengerMessageTypes.PickOpenFile)
		{
			context.Post(_ =>
			{
				var opts = this._messenger.GetOpenFilePickerOptions(id);
				if (opts != null)
				{
					this.MainOpenFilePicker.Filter = opts.FilterString;
				}
				var res = this.MainOpenFilePicker.ShowDialog();
				if (res == DialogResult.OK)
				{
					this._messenger.RespondOpenFilePath(id, this.MainOpenFilePicker.FileName);
				}
			}, null);
		}
	}
}
