using System;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Logging.Serilog;
using GwentTracker.ViewModels;
using GwentTracker.Views;
using Serilog;

namespace GwentTracker
{
    class Program
    {
        public static void Main(string[] args) => BuildAvaloniaApp().Start(Startup, args);

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToDebug()
                .UseReactiveUI();

        // Your application's entry point. Here you can initialize your MVVM framework, DI
        // container, etc.
        private static void Startup(Application app, string[] args)
        {
            var defaultSavePath = ""; // TODO: Get save path from config file
            //var defaultSavePath = Environment.ExpandEnvironmentVariables((ConfigurationManager.AppSettings["defaultSavePath"]));
            var (latestSave, savePath) = GetLatestSave(defaultSavePath);

            if (latestSave == null)
                Log.Warning("No save files (*.sav) found in default save path {path}", savePath);

            var texturePath = ""; // TODO: Get texture path from config file
            FileSystemWatcher watcher = null;
            try
            {
                watcher = new FileSystemWatcher(savePath, "*.sav") { EnableRaisingEvents = true }; // TODO: Get autoload from config file
            }
            catch (Exception e)
            {
                Log.Error(e, "Unable to watch save game directory {directory} for changes", savePath);
            }
            var window = new MainWindow(watcher)
            {
                DataContext = new MainWindowViewModel(latestSave, texturePath)
            };

            app.Run(window);
        }
        
        private static (string, string) GetLatestSave(string path)
        {
            var finalPath = path;
            if (!Directory.Exists(path))
            {
                Log.Warning("Directory {directory} doesn't exist", path);
                var fallback = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "The Witcher 3", "gamesaves");

                if (!Directory.Exists(fallback))
                {
                    Log.Warning("Fallback directory {directory} doesn't exists", fallback);
                    return (null, finalPath);
                }

                path = finalPath = fallback;
            }

            var latestSave = new DirectoryInfo(path).GetFiles("*.sav")
                                                    .OrderByDescending(f => f.LastWriteTime)
                                                    .Select(f => f.FullName)
                                                    .FirstOrDefault();

            return (latestSave, finalPath);
        }
    }
}
