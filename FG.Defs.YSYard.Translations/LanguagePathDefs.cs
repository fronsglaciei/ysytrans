using System.IO;

namespace FG.Defs.YSYard.Translations
{
    public class LanguagePathDefs
    {
        private const string TL_DATA_SERIALIZED = "tldata.json";

        public string PluginRootPath
        {
            get;
        } = "nowhere";

        public string TlDataSerializedPath
            => Path.Combine(this.PluginRootPath, TL_DATA_SERIALIZED);

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
        }
    }
}
