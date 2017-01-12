using System.Configuration;
using System.Linq;
using GwentTracker.ViewModels;
using ReactiveUI;
using System.Reactive.Linq;
using System.Windows;

namespace GwentTracker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IViewFor<MainWindowViewModel>
    {
        public MainWindow()
        {
            ViewModel = new MainWindowViewModel(@"data\test.sav", ConfigurationManager.AppSettings["texturePath"]);
            DataContext = ViewModel;
            
            this.WhenActivated(d =>
            {
                d(this.OneWayBind(this.ViewModel, vm => vm.Cards, v => v.Cards.ItemsSource));
                d(this.OneWayBind(this.ViewModel, vm => vm.Notifications, v => v.Notifications.ItemsSource));
                d(this.Bind(this.ViewModel, vm => vm.FilterString, v => v.FilterString.Text));
                d(this.OneWayBind(this.ViewModel, vm => vm.Filters, v => v.Filters.ItemsSource));
                d(this.OneWayBind(this.ViewModel, vm => vm.LoaderVisibility, v => v.LoadGameProgress.Visibility));
                d(this.BindCommand(this.ViewModel, vm => vm.AddFilter, v => v.AddFilter));
                d(this.WhenAnyValue(v => v.Cards.SelectedItem).BindTo(this, w => w.ViewModel.SelectedCard));
                // Remove Filter binding is done inside xaml since button is part of data template
            });

            InitializeComponent();
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
