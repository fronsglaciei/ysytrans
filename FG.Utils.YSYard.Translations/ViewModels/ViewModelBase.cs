using System.Diagnostics;

namespace FG.Utils.YSYard.Translations.ViewModels;

public class ViewModelBase
{
    public Func<Action, Task>? Invoker { get; set; }

    public Func<Func<Task>, Task>? AsyncInvoker { get; set; }

    protected Task InvokeAsync(Action action)
    {
        if (this.Invoker == null)
        {
            Debug.WriteLine($"[{nameof(ViewModelBase.InvokeAsync)}] {nameof(ViewModelBase.Invoker)}が指定されていません");
            return Task.CompletedTask;
        }
        return this.Invoker(action);
    }

    protected Task InvokeAsync(Func<Task> asyncAction)
    {
        if (this.AsyncInvoker == null)
        {
            Debug.WriteLine($"[{nameof(ViewModelBase.InvokeAsync)}] {nameof(ViewModelBase.AsyncInvoker)}が指定されていません");
            return Task.CompletedTask;
        }
        return this.AsyncInvoker(asyncAction);
    }

    public void OnPageEnter() => this.OnPageEnterInternal();

    protected virtual void OnPageEnterInternal() { }

    public void OnPageExit() => this.OnPageExitInternal();

    protected virtual void OnPageExitInternal() { }
}
