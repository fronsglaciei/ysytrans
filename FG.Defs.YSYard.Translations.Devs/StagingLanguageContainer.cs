namespace FG.Defs.YSYard.Translations.Devs
{
    public class StagingLanguageContainer
    {
        public int Key
        {
            get; set;
        }

        public string Original
        {
            get; set;
        } = string.Empty;

        public string English
        {
            get; set;
        } = string.Empty;

        public string Placeholder
        {
            get; set;
        } = string.Empty;

        public string ApiTranslated
        {
            get; set;
        } = string.Empty;

        public string Japanese
        {
            get; set;
        } = string.Empty;

        public bool IsTranslated
        {
            get; set;
        }
    }
}
