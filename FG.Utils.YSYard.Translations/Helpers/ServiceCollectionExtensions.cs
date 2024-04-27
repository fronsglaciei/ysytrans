using FG.Utils.YSYard.Translations.Contracts.Models;
using FG.Utils.YSYard.Translations.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace FG.Utils.YSYard.Translations.Helpers;

public static class ServiceCollectionExtensions
{
	public static void ConfigureWritable<T>(
		this IServiceCollection services,
        HostBuilderContext context,
        string file = "config.json") where T : class, new()
	{
		services.Configure<T>(context.Configuration);
		services.AddTransient<IWritableOptions<T>>(provider =>
		{
			if (provider.GetService<IConfiguration>() is not IConfigurationRoot configuration)
			{
				throw new ArgumentNullException(nameof(provider));
			}

			var environment = provider.GetService<IHostEnvironment>() ?? throw new ArgumentNullException(nameof(provider));
			var options = provider.GetService<IOptionsMonitor<T>>() ?? throw new ArgumentNullException(nameof(provider));
			return new WritableOptions<T>(environment, options, configuration, file);
		});
	}
}
