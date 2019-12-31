using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using GwentTracker.ViewModels;
using GwentTracker.Views;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace GwentTracker
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
#if SINGLE_FILE            
            var basePath = GetBasePath();
            Directory.SetCurrentDirectory(GetBasePath());
#endif
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Debug()
                .CreateLogger();
            var config = new ConfigurationBuilder()
                .AddIniFile("settings.ini", false)
#if SINGLE_FILE
                .SetBasePath(basePath) // Workaround for https://github.com/dotnet/core-setup/issues/7491
#endif
                .Build();
            var defaultSavePath = Environment.ExpandEnvironmentVariables(config["defaultSavePath"]);
            var (latestSave, savePath) = GetLatestSave(defaultSavePath);

            if (latestSave == null)
                Log.Warning("No save files (*.sav) found in default save path {path}", savePath);

            var texturePath = config["texturePath"];
            var saveDirChanges = ObserveSaveDirChanges(savePath, config.GetValue("autoload", true));
            
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime) 
            {
                desktopLifetime.MainWindow = new MainWindow()
                {
                    DataContext = new MainWindowViewModel(latestSave, texturePath, saveDirChanges)
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
        
        private static string GetBasePath()
        {
            using (var processModule = Process.GetCurrentProcess().MainModule)
                return Path.GetDirectoryName(processModule?.FileName);
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