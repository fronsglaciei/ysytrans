using FG.Utils.YSYard.Translations.Contracts.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace FG.Utils.YSYard.Translations.Models;

public class WritableOptions<T>(
	IHostEnvironment environment,
	IOptionsMonitor<T> options,
	IConfigurationRoot configuration,
	string file) : IWritableOptions<T> where T : class, new()
{
	#region IOptions
	private readonly IHostEnvironment _environment = environment;
	private readonly IOptionsMonitor<T> _options = options;
	private readonly IConfigurationRoot _configuration = configuration;
	#endregion

	private readonly string _file = file;

	public T Value => this._options.CurrentValue;

	public void Update(Action<T> applyChanges)
	{
        var fileProvider = this._environment.ContentRootFileProvider;
        var fileInfo = fileProvider.GetFileInfo(this._file);
        var physicalPath = fileInfo.PhysicalPath;
        if (string.IsNullOrWhiteSpace(physicalPath))
        {
            return;
        }

        var jsonRead = File.ReadAllText(physicalPath, Encoding.UTF8);
		var jobj = JsonSerializer.Deserialize<T>(jsonRead) ?? new T();

        applyChanges(jobj);

		var jsonWrite = JsonSerializer.Serialize(jobj);
        File.WriteAllText(physicalPath, jsonWrite, Encoding.UTF8);

        this._configuration.Reload();
    }
}