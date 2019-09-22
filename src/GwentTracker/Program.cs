using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Logging.Serilog;
using GwentTracker.ViewModels;
using GwentTracker.Views;
using Microsoft.Extensions.Configuration;
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
                .UseDataGrid()
                .UseReactiveUI();

        // Your application's entry point. Here you can initialize your MVVM framework, DI
        // container, etc.
        private static void Startup(Application app, string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Debug()
                .CreateLogger();
            var config = new ConfigurationBuilder()
                .AddIniFile("settings.ini", false)
                .Build();
            var defaultSavePath = Environment.ExpandEnvironmentVariables(config["defaultSavePath"]);
            var (latestSave, savePath) = GetLatestSave(defaultSavePath);

            if (latestSave == null)
                Log.Warning("No save files (*.sav) found in default save path {path}", savePath);

            var texturePath = config["texturePath"];
            var saveDirChanges = ObserveSaveDirChanges(savePath, config.GetValue("autoload", true));
            var window = new MainWindow()
            {
                DataContext = new MainWindowViewModel(latestSave, texturePath, saveDirChanges)
            };

            app.Run(window);
        }

        private static IObservable<string> ObserveSaveDirChanges(string savePath, bool autoload)
        {
            try
            {
                var watcher = new FileSystemWatcher(savePath, "*.sav")
                {
                    EnableRaisingEvents = autoload
                };
                return Observable.Merge(
                        Observable.FromEventPattern<FileSystemEventArgs>(watcher, nameof(FileSystemWatcher.Renamed)),
                        Observable.FromEventPattern<FileSystemEventArgs>(watcher, nameof(FileSystemWatcher.Created)))
                    .Select(e => e.EventArgs.FullPath)
                    .Sample(TimeSpan.FromMilliseconds(100));
            }
            catch (Exception e)
            {
                Log.Error(e, "Unable to watch save game directory {directory} for changes", savePath);
            }

            return Observable.Empty<string>();
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
