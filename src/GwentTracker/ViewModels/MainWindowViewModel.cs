using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
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
using W3SavegameEditor.Core.Common;
using W3SavegameEditor.Core.Savegame;
using W3SavegameEditor.Core.Savegame.Variables;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using ReactiveCommand = ReactiveUI.ReactiveCommand;

namespace GwentTracker.ViewModels
{
    public class MainWindowViewModel : ReactiveObject, ISupportsActivation
    {
        private readonly string _textureStringFormat;

        public ReactiveList<CardViewModel> Cards { get; set; }
        public ReactiveList<Message> Messages { get; set; }
        public Subject<string> Notifications { get; set; }
        public ReactiveCommand<string, SaveGameInfo> Load { get; set; }
        public ReactiveCommand AddFilter { get; set; }
        public ReactiveCommand RemoveFilter { get; set; }
        public ReactiveList<string> Filters { get; set; }

        ObservableAsPropertyHelper<Visibility> _loaderVisibility;
        public Visibility LoaderVisibility => _loaderVisibility.Value;

        private ObservableAsPropertyHelper<Visibility> _cardVisiblity;
        public Visibility CardVisibility => _cardVisiblity.Value;

        private CardViewModel _selectedCard;
        public CardViewModel SelectedCard
        {
            get { return _selectedCard; }
            set { this.RaiseAndSetIfChanged(ref _selectedCard, value); }
        }

        private SaveGameInfo _model;

        private SaveGameInfo Model
        {
            get { return _model; }
            set { this.RaiseAndSetIfChanged(ref _model, value); }
        }

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

        public MainWindowViewModel(string saveGamePath, string textureStringFormat)
        {
            _textureStringFormat = textureStringFormat;
            Activator = new ViewModelActivator();
            Filters = new ReactiveList<string>();
            Cards = new ReactiveList<CardViewModel>();
            Messages = new ReactiveList<Message>();
            Notifications = new Subject<string>();

            this.WhenActivated(d =>
            {
                SaveGamePath = saveGamePath;

                Load = ReactiveCommand.CreateFromTask<string, SaveGameInfo>(LoadSaveGame);
                Load.Subscribe(info =>
                {
                    Model = info;
                    Messages.Add(new Message { Name = "Message 1", Description = "Message 1 Description" });
                    Messages.Add(new Message { Name = "Message 2", Description = "Message 2 Description" });
                    ApplyCards(info.Cards);
                    Notifications.OnNext("Save Game Loaded");
                });
                Load.ThrownExceptions.Subscribe(e => MessageBox.Show(e.ToString()));
                _loaderVisibility = Load.IsExecuting
                    .Select(x => x ? Visibility.Visible : Visibility.Collapsed)
                    .ToProperty(this, x => x.LoaderVisibility, Visibility.Collapsed);

                this.WhenAnyValue(x => x.SaveGamePath)
                    .Select(s => s?.Trim())
                    .DistinctUntilChanged()
                    .Where(s => !string.IsNullOrEmpty(s))
                    .InvokeCommand(Load)
                    .DisposeWith(d);

                _cardVisiblity = this.WhenAnyValue(x => x.SelectedCard)
                    .Select(c => c == null ? Visibility.Collapsed : Visibility.Visible)
                    .ToProperty(this, x => x.CardVisibility, Visibility.Collapsed);

                this.Filters.Changed
                    .Subscribe(i =>
                    {
                        var culture = CultureInfo.CurrentUICulture;
                        var filtered = Filters.Any() ?
                            this.Model.Cards.Where(c => Filters.Any(f => culture.CompareInfo.IndexOf(c.Name, f, CompareOptions.IgnoreCase) >= 0)) :
                            this.Model.Cards;

                        ApplyCards(filtered);
                    })
                    .DisposeWith(d);

                var canAddFilter = this.WhenAnyValue(
                    vm => vm.Model,
                    vm => vm.FilterString,
                    (model, filter) => model?.Cards?.Any() == true && !string.IsNullOrWhiteSpace(filter));
                AddFilter = ReactiveCommand.Create(OnAddFilter, canAddFilter);
                RemoveFilter = ReactiveCommand.Create<string>(OnRemoveFilter);
            });
        }

        private async Task<SaveGameInfo> LoadSaveGame(string path)
        {
            var cards = new List<Card>();
            var saveGameInfo = new SaveGameInfo { Cards = cards };

            if (!File.Exists(path))
                return saveGameInfo;

            var files = new[] {"monsters", "neutral", "nilfgaard", "northernrealms", "scoiatael"};
            
            var deserializer = new DeserializerBuilder()
                                    .IgnoreUnmatchedProperties()
                                    .WithNamingConvention(new CamelCaseNamingConvention())
                                    .Build();

            foreach (var file in files)
            {
                try
                {
                    using (var reader = File.OpenText(Path.Combine("data", $"{file}.yml")))
                    {
                        var contents = await reader.ReadToEndAsync();
                        cards.AddRange(deserializer.Deserialize<List<Card>>(contents));
                    }
                }
                catch (Exception e)
                {
                    // TODO: log
                    throw e;
                }
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

            return saveGameInfo;
        }

        private void OnAddFilter()
        {
            Filters.Add(FilterString);
            FilterString = null;
        }

        private void OnRemoveFilter(string filter)
        {
            Filters.Remove(filter);
        }

        private void ApplyCards(IEnumerable<Card> cards)
        {
            var mapped = cards.Select(c => new CardViewModel(_textureStringFormat)
                                           {
                                               Index = c.Index,
                                               Copies = c.Copies,
                                               Name = c.Name,
                                               Obtained = c.Obtained,
                                               Deck = c.Deck,
                                               Type = c.Type,
                                               Locations = c.Locations
                                           });
            Cards.Clear();
            foreach (var card in mapped)
                Cards.Add(card);
            //Cards.AddRange(filtered);
        }

        public ViewModelActivator Activator { get; }
    }
}
