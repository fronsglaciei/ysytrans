namespace FG.Utils.YSYard.Translations;

internal static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        Startup.Init();
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}
