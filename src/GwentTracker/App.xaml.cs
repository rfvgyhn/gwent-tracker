using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using GwentTracker.Localization;
using GwentTracker.ViewModels;
using GwentTracker.Views;
using Microsoft.Extensions.Configuration;
using NGettext;
using Serilog;
using Splat;

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
            var config = new ConfigurationBuilder()
                .AddIniFile("settings.ini", false)
#if SINGLE_FILE
                .SetBasePath(basePath) // Workaround for https://github.com/dotnet/core-setup/issues/7491
#endif
                .Build();
            var logConfig = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Debug();
            
            if (config.GetValue("logToFile", false))
                logConfig.WriteTo.File("log.txt");
                
            Log.Logger = logConfig.CreateLogger();
            
            var culture = ConfigureLocalization(config["culture"]);
            var defaultSavePath = ExpandEnvVars(config["defaultSavePath"]);
            var (latestSave, savePath) = GetLatestSave(defaultSavePath);

            if (latestSave == null)
                Log.Warning("No save files (*.sav) found in default save path {path}", savePath);

            var fontFamily = config["fontFamily"] ?? "";
            var textureInfo = new TextureInfo(config["texturePath"], 
                                              "textures/{0}.png",
                                              config.GetValue("cacheRemoteTextures", true));
            var saveDirChanges = ObserveSaveDirChanges(savePath, config.GetValue("autoload", true));
            
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime) 
            {
                desktopLifetime.MainWindow = new MainWindow()
                {
                    FontFamily = new FontFamily(fontFamily),
                    DataContext = new MainWindowViewModel(latestSave, textureInfo, saveDirChanges, culture)
                };
            }

            base.OnFrameworkInitializationCompleted();
        }

        private static string ExpandEnvVars(string configPath)
        {
            // Platform checks needed until corefx supports platform specific vars
            // https://github.com/dotnet/corefx/issues/28890
            if (Environment.OSVersion.Platform == PlatformID.Unix ||
                Environment.OSVersion.Platform == PlatformID.MacOSX)
            {
                var path = Regex.Replace(configPath, @"\$(\w+)", "%$1%")
                                .Replace("~", "%HOME%");
                return Environment.ExpandEnvironmentVariables(path);
            }
            
            return Environment.ExpandEnvironmentVariables(configPath);
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
                var fallback = GetFallbackSaveDir();

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

        private static string GetFallbackSaveDir()
        {
            if (Environment.OSVersion.Platform == PlatformID.Unix)
                return ExpandEnvVars("~/.local/share/Steam/steamapps/compatdata/292030/pfx/drive_c/users/steamuser/My Documents/The Witcher 3/gamesaves/"); 
            
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "The Witcher 3", "gamesaves");
        }

        private static CultureInfo ConfigureLocalization(string cultureName)
        {
            var culture = CultureInfo.CurrentUICulture;

            if (!string.IsNullOrEmpty(cultureName))
            {
                try
                {
                    culture = CultureInfo.GetCultureInfo(cultureName);
                    CultureInfo.CurrentUICulture = culture;
                }
                catch (CultureNotFoundException e)
                {
                    Log.Warning($"Invalid culture '{cultureName}' specified in settings. Falling back to system default.");
                }
            }

            var uiCatalog = new Catalog(new MoLoader("gwent-tracker", "locale"), culture);
            var cardsCatalog = new Catalog(new MoLoader("cards", "locale"), culture);
            
            Locator.CurrentMutable.RegisterConstant(uiCatalog, typeof(ICatalog));
            Locator.CurrentMutable.RegisterConstant(cardsCatalog, typeof(ICatalog));

            return culture;
        }
    }
}