using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using GwentTracker.ViewModels;
using ReactiveUI;
using System.Reactive.Linq;
using System.Windows;
using System;

namespace GwentTracker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IViewFor<MainWindowViewModel>
    {
        public MainWindow()
        {
            var defaultSavePath = ConfigurationManager.AppSettings["defaultSavePath"];
            var latestSave = GetLatestSave(defaultSavePath);
            var watcher = new FileSystemWatcher(defaultSavePath, "*.sav")
            {
                EnableRaisingEvents = true
            };
            
            ViewModel = new MainWindowViewModel(latestSave, ConfigurationManager.AppSettings["texturePath"]);
            DataContext = ViewModel;
            
            this.WhenActivated(d =>
            {
                d(this.OneWayBind(this.ViewModel, vm => vm.Cards, v => v.Cards.ItemsSource));
                d(this.OneWayBind(this.ViewModel, vm => vm.Notifications, v => v.Notifications.ItemsSource));
                d(this.Bind(this.ViewModel, vm => vm.FilterString, v => v.FilterString.Text));
                d(this.OneWayBind(this.ViewModel, vm => vm.Filters, v => v.Filters.ItemsSource));
                d(this.OneWayBind(this.ViewModel, vm => vm.LoaderVisibility, v => v.LoadGameProgress.Visibility));
                d(this.OneWayBind(this.ViewModel, vm => vm.CardVisibility, v => v.SelectedCard.Visibility));
                d(this.BindCommand(this.ViewModel, vm => vm.AddFilter, v => v.AddFilter));
                d(this.WhenAnyValue(v => v.Cards.SelectedItem).BindTo(this, w => w.ViewModel.SelectedCard));
                d(Observable.FromEventPattern<FileSystemEventArgs>(watcher, nameof(FileSystemWatcher.Changed))
                            .Select(e => e.EventArgs)
                            .Distinct(e => e.FullPath)
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

        private void OnSaveDirectoryChange(FileSystemEventArgs e)
        {
            ViewModel.SaveGamePath = e.FullPath;
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
