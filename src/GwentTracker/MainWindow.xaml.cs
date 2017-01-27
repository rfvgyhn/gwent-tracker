using GwentTracker.Properties;
using GwentTracker.ViewModels;
using ReactiveUI;
using ReactiveUI.Events;
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
            var defaultSavePath = Environment.ExpandEnvironmentVariables((ConfigurationManager.AppSettings["defaultSavePath"]));
            var latestSave = GetLatestSave(defaultSavePath);
            var watcher = new FileSystemWatcher(defaultSavePath, "*.sav")
            {
                EnableRaisingEvents = Settings.Default.AutoLoad,
            };
            
            ViewModel = new MainWindowViewModel(latestSave, ConfigurationManager.AppSettings["texturePath"]);
            DataContext = ViewModel;
            
            this.WhenActivated(d =>
            {
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
                d(Observable.Merge(
                        Observable.FromEventPattern<FileSystemEventArgs>(watcher, nameof(FileSystemWatcher.Renamed)),
                        Observable.FromEventPattern<FileSystemEventArgs>(watcher, nameof(FileSystemWatcher.Created)))
                    .Select(e => e.EventArgs.FullPath)
                    .Distinct()
                    .Subscribe(OnSaveDirectoryChange));
                // Remove Filter binding is done inside xaml since button is part of data template
            });

            InitializeComponent();
        }

        private string GetLatestSave(string path)
        {
            if (!Directory.Exists(path))
                return null;

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
