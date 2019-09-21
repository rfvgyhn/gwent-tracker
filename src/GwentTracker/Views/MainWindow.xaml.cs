using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using GwentTracker.ViewModels;
using ReactiveUI;

namespace GwentTracker.Views
{
    public class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        private DataGrid Cards => this.FindControl<DataGrid>("Cards");
        //private ItemsControl Messages => this.FindControl<ItemsControl>("Messages");
        private TextBox FilterString => this.FindControl<TextBox>("FilterString");
        private ItemsControl Filters => this.FindControl<ItemsControl>("Filters");
        private ProgressBar LoadGameProgress => this.FindControl<ProgressBar>("LoadGameProgress");
        private Grid SelectedCard => this.FindControl<Grid>("SelectedCard");
        private Button AddFilter => this.FindControl<Button>("AddFilter");
        
        public MainWindow()
        {
            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.Cards, v => v.Cards.Items).DisposeWith(d);
                //this.OneWayBind(ViewModel, vm => vm.Messages, v => v.Messages.Items).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.FilterString, v => v.FilterString.Text).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.Filters, v => v.Filters.Items).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.LoaderVisibility, v => v.LoadGameProgress.IsVisible).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.CardVisibility, v => v.SelectedCard.IsVisible).DisposeWith(d);
                ViewModel.Notifications.Subscribe(Notify).DisposeWith(d);
                this.WhenAnyValue(v => v.Cards.SelectedItem).BindTo(this, w => w.ViewModel.SelectedCard).DisposeWith(d);
                this.WhenAnyValue(v => v.ViewModel.LoadCards).SelectMany(x => x.Execute()).Subscribe().DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.AddFilter, v => v.AddFilter).DisposeWith(d);
                Observable
                    .FromEventPattern<EventHandler<KeyEventArgs>, KeyEventArgs>(
                        handler => FilterString.KeyDown += handler,
                        handler => FilterString.KeyDown -= handler)
                    .Where(e => e.EventArgs.Key == Key.Enter)
                    .Select(e => Unit.Default)
                    .InvokeCommand(this, v => v.ViewModel.AddFilter)
                    .DisposeWith(d);
                // Remove Filter binding is done inside xaml since button is part of data template
            });
            InitializeComponent();
        }
        
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void Notify(string message)
        {
            //this.Notifications.MessageQueue.Enqueue(message);
        }

//        object IViewFor.ViewModel
//        {
//            get { return ViewModel; }
//            set { ViewModel = (MainWindowViewModel)value; }
//        }
    }
}