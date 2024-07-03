using FG.Utils.YSYard.Translations.Contracts.Services;
using FG.Utils.YSYard.Translations.Helpers;
using FG.Utils.YSYard.Translations.Models;
using FG.Utils.YSYard.Translations.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MudBlazor;
using MudBlazor.Services;

namespace FG.Utils.YSYard.Translations;

public class HostWrapper : IDisposable
{
    private readonly IHost _host =
        Host
        .CreateDefaultBuilder()
        .ConfigureAppConfiguration(WireupConfigurations)
        .ConfigureServices(WireupServices)
        .Build();

    public IServiceProvider Services => this._host.Services;

    private static void WireupConfigurations(IConfigurationBuilder configs)
        => configs.AddJsonFile(".config/config.json");

    private static void WireupServices(HostBuilderContext context, IServiceCollection services)
    {
        services.AddWindowsFormsBlazorWebView();
        services.AddMudServices(config =>
        {
            config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.TopRight;
        });
        services.AddSingleton<ICustomDialogService, CustomDialogService>();
        services.AddSingleton<IKeyNotificationService, KeyNotificationService>();
        services.AddSingleton<ILanguageRepositoryService, LanguageRepositoryService>();
        services.AddSingleton<ITranslationRepositoryService, TranslationRepositoryService>();
        services.AddSingleton<IStoryRepositoryService, StoryRepositoryService>();
        services.AddSingleton<ITranslationApiService, GoogleLanguageApiService>();
        services.AddSingleton<IIgnoreListService, IgnoreListService>();

        services.ConfigureWritable<AppConfig>(context, ".config/config.json");

#if DEBUG
        services.AddBlazorWebViewDeveloperTools();
#endif
    }

    public T? GetService<T>() => this._host.Services.GetService<T>();

    public void Dispose()
    {
        this._host.Dispose();
        GC.SuppressFinalize(this);
    }
}
