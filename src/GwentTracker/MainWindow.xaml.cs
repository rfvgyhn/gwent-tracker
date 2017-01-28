using GwentTracker.Properties;
using GwentTracker.ViewModels;
using ReactiveUI;
using ReactiveUI.Events;
using Serilog;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;

namespace GwentTracker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : IViewFor<MainWindowViewModel>
    {
        public MainWindow()
        {
            var savePath = Environment.ExpandEnvironmentVariables((ConfigurationManager.AppSettings["defaultSavePath"]));
            var latestSave = GetLatestSave(savePath, out savePath);

            if (latestSave == null)
                Log.Warning("No save files (*.sav) found in default save path {path}", savePath);
            
            ViewModel = new MainWindowViewModel(latestSave, ConfigurationManager.AppSettings["texturePath"]);
            DataContext = ViewModel;
            
            this.WhenActivated(d =>
            {
                FileSystemWatcher watcher = null;
                try
                {
                    watcher = new FileSystemWatcher(savePath, "*.sav")
                    {
                        EnableRaisingEvents = Settings.Default.AutoLoad,
                    };
                }
                catch (Exception e)
                {
                    Log.Error(e, "Unable to watch save game directory {directory} for changes", savePath);
                    Notify("Unable to watch save game directory for changes");
                }
                d(this.OneWayBind(ViewModel, vm => vm.Cards, v => v.Cards.ItemsSource));
                d(this.OneWayBind(ViewModel, vm => vm.Messages, v => v.Messages.ItemsSource));
                d(this.Bind(ViewModel, vm => vm.FilterString, v => v.FilterString.Text));
                d(this.OneWayBind(ViewModel, vm => vm.Filters, v => v.Filters.ItemsSource));
                d(this.OneWayBind(ViewModel, vm => vm.LoaderVisibility, v => v.LoadGameProgress.Visibility));
                d(this.OneWayBind(ViewModel, vm => vm.CardVisibility, v => v.SelectedCard.Visibility));
                d(ViewModel.Notifications.Subscribe(Notify));
                d(this.WhenAnyValue(v => v.Cards.SelectedItem).BindTo(this, w => w.ViewModel.SelectedCard));
                d(this.WhenAnyValue(v => v.ViewModel.LoadCards).SelectMany(x => x.Execute()).Subscribe());
                d(this.BindCommand(ViewModel, vm => vm.AddFilter, v => v.AddFilter));
                d(this.Events().KeyDown
                      .Where(e => e.Key == Key.Enter && e.Source == FilterString)
                      .Select(e => Unit.Default)
                      .InvokeCommand(this, v => v.ViewModel.AddFilter));

                if (watcher != null)
                {
                    d(Observable.Merge(
                            Observable.FromEventPattern<FileSystemEventArgs>(watcher, nameof(FileSystemWatcher.Renamed)),
                            Observable.FromEventPattern<FileSystemEventArgs>(watcher, nameof(FileSystemWatcher.Created)))
                        .Select(e => e.EventArgs.FullPath)
                        .Distinct()
                        .Subscribe(OnSaveDirectoryChange));
                }
                // Remove Filter binding is done inside xaml since button is part of data template
            });

            InitializeComponent();
        }

        private string GetLatestSave(string path, out string finalPath)
        {
            finalPath = path;
            if (!Directory.Exists(path))
            {
                Log.Warning("Directory {directory} doesn't exist", path);
                var fallback = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "The Witcher 3", "gamesaves");

                if (!Directory.Exists(fallback))
                {
                    Log.Warning("Fallback directory {directory} doesn't exists", fallback);
                    return null;
                }

                path = finalPath = fallback;
            }

            return new DirectoryInfo(path).GetFiles("*.sav")
                                          .OrderByDescending(f => f.LastWriteTime)
                                          .Select(f => f.FullName)
                                          .FirstOrDefault();
        }

        private void Notify(string message)
        {
            this.Notifications.MessageQueue.Enqueue(message);
        }

        private void OnSaveDirectoryChange(string path)
        {
            ViewModel.SaveGamePath = path;
        }

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (MainWindowViewModel)value; }
        }

        public MainWindowViewModel ViewModel { get; set; }
        
        
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(MainWindowViewModel), typeof(MainWindow));
    }
}
