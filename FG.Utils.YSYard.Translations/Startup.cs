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

public static class Startup
{
    public static IServiceProvider? Services { get; private set; }

    public static void Init()
    {
        var host = Host
            .CreateDefaultBuilder()
            .ConfigureAppConfiguration(WireupConfigurations)
            .ConfigureServices(WireupServices)
            .Build();
        Services = host.Services;
    }

    private static void WireupConfigurations(IConfigurationBuilder configs)
    {
        configs.AddJsonFile(".config/config.json");
    }

    private static void WireupServices(HostBuilderContext context, IServiceCollection services)
    {
        services.AddWindowsFormsBlazorWebView();
        services.AddMudServices(config =>
        {
            config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.TopRight;
        });
        services.AddSingleton<IFormMessengerService, FormMessengerService>();
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

    // force get a non-nullable service
    public static T GetService<T>() => Services!.GetService<T>()!;
}
