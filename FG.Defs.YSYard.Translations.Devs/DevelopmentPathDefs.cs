using System.IO;

namespace FG.Defs.YSYard.Translations.Devs
{
    public class DevelopmentPathDefs
    {
        private const string STAGING_JSON = "staging.json";

        private const string CACHE_EXT = ".tmp";

        private const string STORY_JSON = "story.json";

        public string PluginRootPath
        {
            get;
        } = "nowhere";

        public string StagingJsonPath
        {
            get;
        } = "nowhere";

        public string StagingCachePath
        {
            get;
        } = "nowhere";

        public string StoryPath
        {
            get;
        } = "nowhere";

        public bool IsValid
        {
            get
            {
                if (Directory.Exists(this.PluginRootPath))
                {
                    var assemblyFileName = Path.GetFileName(typeof(DevelopmentPathDefs).Assembly.Location);
                    var path = Path.Combine(this.PluginRootPath, assemblyFileName);
                    if (File.Exists(path))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public DevelopmentPathDefs(string rootPath)
        {
            this.PluginRootPath = rootPath;
            this.StagingJsonPath = Path.Combine(rootPath, STAGING_JSON);
            this.StagingCachePath = Path.Combine(rootPath, $"{STAGING_JSON}{CACHE_EXT}");
            this.StoryPath = Path.Combine(rootPath, STORY_JSON);
        }

        public void EnsureAllCreated()
        {
            if (!Directory.Exists(this.PluginRootPath))
            {
                throw new DirectoryNotFoundException($"{nameof(PluginRootPath)} does not exist : {this.PluginRootPath}");
            }
        }
    }
}
