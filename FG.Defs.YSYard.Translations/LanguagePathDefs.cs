using System.IO;

namespace FG.Defs.YSYard.Translations
{
    public class LanguagePathDefs
	{
		private const string LANGUAGES_FOLDER = "Languages";

		private const string LANGUAGE_TALKS_FOLDER = "LanguageTalks";

		private const string EXPORTED_FOLDER = "Exported";

		private const string STORIES_FOLDER = "Stories";

		private const int LANGUAGES_MAX_DIGITS = 8;

		private const int LANGUAGE_TALKS_MAX_DIGITS = 7;

		private const int STORIES_MAX_DIGITS = 7;

		private const string TRANSLATED_LANGUAGES_SERIALIZED = "translatedLanguages.json";

		private const string TRANSLATED_LANGUAGE_TALKS_SERIALIZED = "translatedLanguageTalks.json";

		public string PluginRootPath { get; } = "nowhere";

		public string LanguagesPath { get; } = "nowhere";

		public string LanguagesExportedPath { get; } = "nowhere";

		public string LanguageTalksPath { get; } = "nowhere";

		public string LanguageTalksExportedPath { get; } = "nowhere";

		public string StoriesPath { get; } = "nowhere";

		public string TranslatedLanguagesSerializedPath
			=> Path.Combine(this.PluginRootPath, TRANSLATED_LANGUAGES_SERIALIZED);

		public string TranslatedLanguageTalksSerializedPath
			=> Path.Combine(this.PluginRootPath, TRANSLATED_LANGUAGE_TALKS_SERIALIZED);

		public bool IsValid
		{
			get
			{
				if (Directory.Exists(this.PluginRootPath))
				{
					var assemblyFileName = Path.GetFileName(typeof(LanguagePathDefs).Assembly.Location);
					var path = Path.Combine(this.PluginRootPath, assemblyFileName);
					if (File.Exists(path))
					{
						return true;
					}
				}
				return false;
			}
		}

		public LanguagePathDefs(string rootPath)
		{
			this.PluginRootPath = rootPath;
			this.LanguagesPath = Path.Combine(rootPath, LANGUAGES_FOLDER);
			this.LanguagesExportedPath = Path.Combine(this.LanguagesPath, EXPORTED_FOLDER);
			this.LanguageTalksPath = Path.Combine(rootPath, LANGUAGE_TALKS_FOLDER);
			this.LanguageTalksExportedPath = Path.Combine(this.LanguageTalksPath, EXPORTED_FOLDER);
			this.StoriesPath = Path.Combine(rootPath, STORIES_FOLDER);
		}

		public LanguagePathDefs() { }

		public void EnsureAllCreated()
		{
			if (!Directory.Exists(this.PluginRootPath))
			{
				throw new DirectoryNotFoundException($"{nameof(PluginRootPath)} does not exist : {this.PluginRootPath}");
			}

			if (!Directory.Exists(this.LanguagesPath))
			{
				Directory.CreateDirectory(this.LanguagesPath);
			}
			if (!Directory.Exists(this.LanguagesExportedPath))
			{
				Directory.CreateDirectory(this.LanguagesExportedPath);
			}
			if (!Directory.Exists(this.LanguageTalksPath))
			{
				Directory.CreateDirectory(this.LanguageTalksPath);
			}
			if (!Directory.Exists(this.LanguageTalksExportedPath))
			{
				Directory.CreateDirectory(this.LanguageTalksExportedPath);
			}
			if (!Directory.Exists(this.StoriesPath))
			{
				Directory.CreateDirectory(this.StoriesPath);
			}
		}

		public string GetTranslationFilePath(KeyNotification kn)
		{
			if (kn.KeyType == LanguageKeyTypes.Language)
			{
				var filename = kn.Key.ToString().PadLeft(LANGUAGES_MAX_DIGITS, '0');
				return Path.Combine(this.LanguagesPath, $"{filename}.txt");
			}
			else if (kn.KeyType == LanguageKeyTypes.LanguageTalk)
			{
				var filename = kn.Key.ToString().PadLeft(LANGUAGE_TALKS_MAX_DIGITS, '0');
				return Path.Combine(this.LanguageTalksPath, $"{filename}.txt");
			}
			return string.Empty;
		}

		public string GetExportedFilePath(KeyNotification kn)
		{
			if (kn.KeyType == LanguageKeyTypes.Language)
			{
				var filename = kn.Key.ToString().PadLeft(LANGUAGES_MAX_DIGITS, '0');
				return Path.Combine(this.LanguagesExportedPath, $"{filename}.json");
			}
			else if (kn.KeyType == LanguageKeyTypes.LanguageTalk)
			{
				var filename = kn.Key.ToString().PadLeft(LANGUAGE_TALKS_MAX_DIGITS, '0');
				return Path.Combine(this.LanguageTalksExportedPath, $"{filename}.json");
			}
			return string.Empty;
		}

		public string GetStoryFilePath(int id)
		{
			var filename = id.ToString().PadLeft(STORIES_MAX_DIGITS, '0');
			return Path.Combine(this.StoriesPath, $"{filename}.json");
		}

		public bool TryGetKeyFromPath(string path, out int key)
		{
			var filename = Path.GetFileNameWithoutExtension(path);
			return int.TryParse(filename, out key);
		}
	}
}
