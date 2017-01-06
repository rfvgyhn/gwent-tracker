using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;
using GwentTracker.Model;
using ReactiveUI;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Data;
using ReactiveUI.Legacy;
using ServiceStack;
using W3SavegameEditor.Core.Common;
using W3SavegameEditor.Core.Savegame;
using W3SavegameEditor.Core.Savegame.Variables;
using ReactiveCommand = ReactiveUI.ReactiveCommand;

namespace GwentTracker.ViewModels
{
    public class MainWindowViewModel : ReactiveObject, ISupportsActivation
    {
        private ObservableAsPropertyHelper<SaveGameInfo> _model;
        private SaveGameInfo Model => _model.Value;
        private ObservableAsPropertyHelper<List<CardViewModel>> _cards;
        public List<CardViewModel> Cards => _cards.Value;
        public ReactiveCommand<string, SaveGameInfo> Load { get; set; }
        public ReactiveCommand AddFilter { get; set; }
        public ReactiveList<string> Filters { get; set; }

        ObservableAsPropertyHelper<Visibility> _loaderVisibility;
        public Visibility LoaderVisibility => _loaderVisibility.Value;

        private string _filterString;
        public string FilterString
        {
            get { return _filterString; }
            set { this.RaiseAndSetIfChanged(ref _filterString, value); }
        }

        private string _saveGamePath;
        public string SaveGamePath
        {
            get { return _saveGamePath; }
            set { this.RaiseAndSetIfChanged(ref _saveGamePath, value); }
        }

        public MainWindowViewModel(string saveGamePath)
        {
            Activator = new ViewModelActivator();
            Filters = new ReactiveList<string>();

            this.WhenActivated(d =>
            {
                SaveGamePath = saveGamePath;

                Load = ReactiveCommand.CreateFromTask<string, SaveGameInfo>(LoadSaveGame);
                Load.ThrownExceptions.Subscribe(e => MessageBox.Show(e.ToString()));
                _model = Load.ToProperty(this, vm => vm.Model);
                _loaderVisibility = Load.IsExecuting
                    .Select(x => x ? Visibility.Visible : Visibility.Collapsed)
                    .ToProperty(this, x => x.LoaderVisibility, Visibility.Collapsed);

                _cards = this.WhenAnyValue(x => x.Model)
                    .Select(i => i?.Cards?.Select(c => new CardViewModel { Index = c.Index, Copies = c.Copies, Name = c.Name, Obtained = c.Obtained }).ToList())
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .ToProperty(this, vm => vm.Cards)
                    .DisposeWith(d);

                this.WhenAnyValue(x => x.SaveGamePath)
                    .Select(s => s?.Trim())
                    .DistinctUntilChanged()
                    .Where(s => !string.IsNullOrEmpty(s))
                    .InvokeCommand(Load)
                    .DisposeWith(d);

                var canAddFilter = this.WhenAnyValue(
                    vm => vm.Model,
                    vm => vm.FilterString,
                    (model, filter) => model?.Cards?.Any() == true && !string.IsNullOrWhiteSpace(filter));
                AddFilter = ReactiveCommand.Create(OnAddFilter, canAddFilter);
            });
        }

        private async Task<SaveGameInfo> LoadSaveGame(string path)
        {
            List<Card> cards;

            using (var reader = File.OpenText(@"data\cards.csv"))
            {
                var csv = await reader.ReadToEndAsync();
                cards = csv.FromCsv<List<Card>>();
            }

            var saveGame = await SavegameFile.ReadAsync(path);
            var cardCollection = ((BsVariable)saveGame.Variables[11]).Variables
                                                                     .Skip(2)
                                                                     .TakeWhile(v => v.Name != "SBSelectedDeckIndex")
                                                                     .Where(v => v.Name == "cardIndex" || v.Name == "numCopies")
                                                                     .ToArray();
            for (var i = 0; i < cardCollection.Length; i += 2)
            {
                var index = ((VariableValue<int>)((VlVariable)cardCollection[i]).Value).Value;
                var copies = ((VariableValue<int>)((VlVariable)cardCollection[i + 1]).Value).Value;
                var card = cards.SingleOrDefault(c => c.Index == index);

                if (card != null)
                {
                    card.Copies = copies;
                    card.Obtained = true;
                }
            }

            return new SaveGameInfo
            {
                Cards = cards
            };
        }

        private void OnAddFilter()
        {
            Filters.Add(FilterString);
            FilterString = null;
        }

        public ViewModelActivator Activator { get; }
    }
}
