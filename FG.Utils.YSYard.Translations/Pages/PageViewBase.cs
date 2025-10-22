using FG.Utils.YSYard.Translations.ViewModels;
using Microsoft.AspNetCore.Components;

namespace FG.Utils.YSYard.Translations.Pages;

public class PageViewBase : ComponentBase, IDisposable
{
    private ViewModelBase? _pageViewModel;

    protected void SetupViewmodel(ViewModelBase viewModel)
    {
        viewModel.Invoker = x => this.InvokeAsync(() =>
        {
            x.Invoke();
            this.StateHasChanged();
        });
        viewModel.AsyncInvoker = x => this.InvokeAsync(async () =>
        {
            await x.Invoke();
            this.StateHasChanged();
        });
        viewModel.OnPageEnter();
        this._pageViewModel = viewModel;
    }

    public void Dispose()
    {
        this._pageViewModel?.OnPageExit();
        this.DisposeInternal();
        GC.SuppressFinalize(this);
    }

    protected virtual void DisposeInternal() { }
}
