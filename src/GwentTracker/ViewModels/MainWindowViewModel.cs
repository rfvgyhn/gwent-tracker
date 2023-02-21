using GwentTracker.Model;
using ReactiveUI;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Avalonia.ReactiveUI;
using DynamicData;
using DynamicData.Binding;
using GwentTracker.Localization;
using W3SavegameEditor.Core.Savegame;
using W3SavegameEditor.Core.Savegame.Variables;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using ReactiveCommand = ReactiveUI.ReactiveCommand;

namespace GwentTracker.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
        private readonly CultureInfo _cultureInfo;
        private readonly SourceList<CardViewModel> _cards;
        private readonly ReadOnlyObservableCollection<CardViewModel> _filteredCards;
        private readonly Translate _t;
        private bool _initialLoadComplete = false;
        private Dictionary<int, int> _summons;

        public ReadOnlyObservableCollection<CardViewModel> Cards => _filteredCards;
        public ObservableCollection<MissableInfo> Messages { get; set; }
        public Subject<string> Notifications { get; set; }
        public TimeSpan NotificationDuration => TimeSpan.FromSeconds(5);
        public ReactiveCommand<string, SaveGameInfo> Load { get; set; }
        public ReactiveCommand<Unit, (IEnumerable<Card>, IEnumerable<MissableInfo>)> LoadCards { get; set; }
        public ReactiveCommand<Unit, Unit> AddFilter { get; set; }
        public ReactiveCommand<string, Unit> RemoveFilter { get; set; }
        public ObservableCollection<string> Filters { get; set; }

        private ObservableAsPropertyHelper<bool> _loaderVisibility;
        public bool LoaderVisibility => _loaderVisibility.Value;

        private ObservableAsPropertyHelper<bool> _cardVisibility;
        public bool CardVisibility => _cardVisibility.Value;
        
        private CardViewModel _selectedCard;
        public CardViewModel SelectedCard
        {
            get => _selectedCard;
            set => this.RaiseAndSetIfChanged(ref _selectedCard, value);
        }

        private SaveGameInfo _model;

        private SaveGameInfo Model
        {
            get => _model;
            set => this.RaiseAndSetIfChanged(ref _model, value);
        }

        private string _filterString;
        public string FilterString
        {
            get => _filterString;
            set => this.RaiseAndSetIfChanged(ref _filterString, value);
        }

        private string _saveGamePath;
        public string SaveGamePath
        {
            get => _saveGamePath;
            set => this.RaiseAndSetIfChanged(ref _saveGamePath, value);
        }

        private CollectionProgress _baseGameProgress;
        public CollectionProgress BaseGameProgress
        {
            get => _baseGameProgress;
            set => this.RaiseAndSetIfChanged(ref _baseGameProgress, value);
        }
        
        private CollectionProgress _bAndWProgress;
        public CollectionProgress BandWProgress
        {
            get => _bAndWProgress;
            set => this.RaiseAndSetIfChanged(ref _bAndWProgress, value);
        }
        
        private CollectionProgress _dlcProgress;
        public CollectionProgress DlcProgress
        {
            get => _dlcProgress;
            set => this.RaiseAndSetIfChanged(ref _dlcProgress, value);
        }

        public MainWindowViewModel(string saveGamePath, TextureInfo textureInfo, IObservable<string> saveDirChanges, CultureInfo cultureInfo)
        {
            _t = new Translate();
            _cultureInfo = cultureInfo;
            Activator = new ViewModelActivator();
            Filters = new ObservableCollection<string>();
            _cards = new SourceList<CardViewModel>();
            _summons = new Dictionary<int, int>();
            Messages = new ObservableCollection<MissableInfo>();
            Notifications = new Subject<string>();
            BaseGameProgress = new CollectionProgress();
            BandWProgress = new CollectionProgress();
            DlcProgress = new CollectionProgress();

            var filterChanged = Filters.ObserveCollectionChanges();
            filterChanged.Subscribe(_ =>
            {
                foreach (var card in _cards.Items)
                    card.IsHidden = ShouldFilterCard(card);
            });
            _cards.Connect()
                .AutoRefreshOnObservable(_ => filterChanged)
                .Filter(x => !x.IsHidden)
                .ObserveOn(AvaloniaScheduler.Instance)
                .Bind(out _filteredCards)
                .Subscribe();

            LoadCards = ReactiveCommand.CreateFromTask(LoadCardsFromFiles);
            LoadCards.ThrownExceptions.Subscribe(e =>
            {
                Log.Error(e, "Unable to load card data");
                Notifications.OnNext(_t["Unable to load card info"]);
            });
            LoadCards.Subscribe(items =>
            {
                var (cardInfo, missables) = items;
                var cards = cardInfo as List<Card> ?? cardInfo.ToList();
                var mapped = cards.Select(c => new CardViewModel(textureInfo)
                {
                    Index = c.Index,
                    Copies = c.Copies,
                    MaxCopies = c.MaxCopies,
                    Name = c.Name,
                    Flavor = c.Flavor,
                    Obtained = c.Obtained,
                    Deck = c.Deck,
                    Type = c.Type,
                    Locations = c.Locations,
                    Source = c.Source
                });

                _cards.Clear();
                foreach (var card in mapped)
                    _cards.Add(card);

                Messages.Clear();
                Messages.AddRange(missables);
                SaveGamePath = saveGamePath;
                
                var requiredBase = cards.Where(c => c.Source == CardSource.BaseGame).Select(c => c.Name).Distinct().Count();
                var totalBase = cards.Where(c => c.Source == CardSource.BaseGame).Sum(c => c.MaxCopies);
                var requiredBandW = cards.Where(c => c.Source == CardSource.BloodAndWine).Sum(c => c.MaxCopies);
                var totalHoS = cards.Where(c => c.Source == CardSource.HeartsOfStone).Sum(c => c.MaxCopies);

                BaseGameProgress.Needed = requiredBase;
                BaseGameProgress.Total = totalBase;
                BandWProgress.Needed = requiredBandW;
                BandWProgress.Total = requiredBandW;
                DlcProgress.Total = BandWProgress.Total + totalHoS;
            });

            Load = ReactiveCommand.CreateFromTask<string, SaveGameInfo>(LoadSaveGame);
            Load.Subscribe(OnSaveGameLoaded);
            Load.ThrownExceptions.Subscribe(e =>
            {
                Log.Error(e, "Unable to load save game at {path}", SaveGamePath);
                Notifications.OnNext(_t["Unable to load save game"]);
            });
            _loaderVisibility = Load.IsExecuting
                .Select(x => x)
                .ToProperty(this, x => x.LoaderVisibility);

            this.WhenAnyValue(x => x.SaveGamePath)
                .Select(s => s?.Trim())
                .DistinctUntilChanged()
                .Where(s => !string.IsNullOrEmpty(s))
                .InvokeCommand(Load);

            this.WhenAnyValue(v => v.SelectedCard.LoadTexture)
                .SelectMany(x => x.Execute())
                .Subscribe();

            _cardVisibility = this.WhenAnyValue(x => x.SelectedCard)
                .Select(c => c != null)
                .ToProperty(this, x => x.CardVisibility);

            var canAddFilter = this.WhenAnyValue(vm => vm.FilterString, filter => !string.IsNullOrWhiteSpace(filter));
            AddFilter = ReactiveCommand.Create(OnAddFilter, canAddFilter);
            RemoveFilter = ReactiveCommand.Create<string>(OnRemoveFilter);

            saveDirChanges?.Subscribe(OnSaveDirectoryChange);
        }

        private void OnSaveDirectoryChange(string path)
        {
            Log.Debug("Save directory change '{path}'", path);
            SaveGamePath = path;
        }
        
        private async Task<(IEnumerable<Card>, IEnumerable<MissableInfo>)> LoadCardsFromFiles()
        {
            var cards = new List<Card>();
            var missables = new List<MissableInfo>();
            var files = new[] { "monsters", "neutral", "nilfgaard", "northernrealms", "scoiatael", "skellige" };
            var deserializer = new DeserializerBuilder()
                                    .IgnoreUnmatchedProperties()
                                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                                    .WithTypeConverter(new TranslateStringConverter())
                                    .Build();
            string filePath;
            foreach (var file in files)
            {
                filePath = Path.Combine("data", $"{file}.yml");
                try
                {
                    using (var reader = File.OpenText(filePath))
                    {
                        var contents = await reader.ReadToEndAsync();
                        var deserializedCards = deserializer.Deserialize<List<Card>>(contents);
                        
                        foreach (var card in deserializedCards.Where(c => c.AttachedTo != null))
                        {
                            card.MaxCopies = deserializedCards.Where(c => c.Index == card.AttachedTo.Value).Single().MaxCopies;
                            _summons.Add(card.AttachedTo.Value, card.Index);
                        }
                        
                        cards.AddRange(deserializedCards);
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e, "Couldn't load card data from {file}", filePath);
                    throw;
                }
            }

            filePath = Path.Combine("data", "missable.yml");
            try
            {
                using (var reader = File.OpenText(filePath))
                {
                    var contents = await reader.ReadToEndAsync();
                    missables.AddRange(deserializer.Deserialize<List<MissableInfo>>(contents));
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "Couldn't load missable card data from {file}", filePath);
                throw;
            }
            

            return (cards, missables);
        }

        private async Task<SaveGameInfo> LoadSaveGame(string path)
        {
            var saveGame = await SavegameFile.ReadAsync(path);
            var cardCollection = 
                ((BsVariable)saveGame.Variables.Single(v => v.Name == "CR4GwintManager"))
                .Variables
                .Skip(2)
                .TakeWhile(v => v.Name != "SBSelectedDeckIndex")
                .Where(v => v.Name == "cardIndex" || v.Name == "numCopies")
                .ToArray();
            var cards = new List<KeyValuePair<int, int>>(cardCollection.Length);
            for (var i = 0; i < cardCollection.Length; i += 2)
            {
                var index = ((VariableValue<int>)((VlVariable)cardCollection[i]).Value).Value;
                var copies = ((VariableValue<int>)((VlVariable)cardCollection[i + 1]).Value).Value;
                cards.Add(new KeyValuePair<int, int>(index, copies));
            }

            // TODO: figure out how quest info is stored and pull out the quests that cause missable cards

            return new SaveGameInfo
            {
                CardCopies = cards
            };
        }

        private void OnSaveGameLoaded(SaveGameInfo info)
        {
            Model = info;
            var newCards = new List<string>();
            foreach (var (index, copies) in info.CardCopies)
            {
                var card = _cards.Items.Where(c => c.Index == index).SingleOrDefault();

                if (card != null)
                {
                    CardViewModel summon = null;
                    
                    if (_summons.ContainsKey(index))
                        summon = _cards.Items.Where(c => c.Index == _summons[index]).SingleOrDefault();

                    if (_initialLoadComplete && (card.Obtained == false || card.Copies < copies))
                    {
                        newCards.Add(card.Name);
                        
                        if (summon != null)
                            newCards.Add(summon.Name);
                    }

                    card.Obtained = true;
                    card.Copies = copies;

                    if (summon != null)
                    {
                        summon.Obtained = true;
                        summon.Copies = copies;
                    }
                    
                    // Some save files report Francesca Findabair: the Beautiful as having multiple copies.
                    // Since this is a leader card and you can only effectively have one copy, manually
                    // fix it here.
                    if (card.Index == 3003 && card.Copies > 1)
                        card.Copies = 1;
                }
            }
            
            foreach (var missable in Messages.Where(m => m.State == MissableState.Active))
            {
                var obtained = _cards.Items.Where(c => missable.CardIds.Contains(c.Index)).All(c => c.Obtained);
                if (obtained)
                    missable.State = MissableState.Obtained;
                else
                {
                    // TODO: Check if quest status is completed/failed and set missable.state = Missed 
                }
            }

            if (newCards.Any())
            {
                var cardNames = string.Join(", ", newCards);
                Notifications.OnNext(_t[$"Obtained {cardNames}"]);
            }

            BaseGameProgress.Copies = GetCopies(CardSource.BaseGame);
            BaseGameProgress.Collected = GetCollected(CardSource.BaseGame);
            BandWProgress.Collected = GetCopies(CardSource.BloodAndWine);
            DlcProgress.Copies = GetCopies(CardSource.Dlc);
            DlcProgress.Collected =  GetCollected(CardSource.Dlc, false);
            _initialLoadComplete = true;
        }

        private int GetCopies(CardSource source)
        {
            return _cards.Items
                         .Where(c => c.Obtained && (c.Source & source) > 0)
                         .Sum(c => c.Copies);
        }

        private int GetCollected(CardSource source, bool distinctOnly = true)
        {
            var cards = _cards.Items
                              .Where(c => c.Obtained && (c.Source & source) > 0)
                              .Select(c => c.Name);
            
            if (distinctOnly)
                cards = cards.Distinct();
                    
            return cards.Count();
        }
        
        private bool ShouldFilterCard(CardViewModel card)
        {
            var compareInfo = _cultureInfo.CompareInfo;

            return Filters.Any() &&
                   !Filters.All(f => compareInfo.IndexOf(card.Name, f, CompareOptions.IgnoreCase) >= 0 ||
                                     compareInfo.IndexOf(card.Deck, f, CompareOptions.IgnoreCase) >= 0 ||
                                     compareInfo.IndexOf(card.Type ?? "", f, CompareOptions.IgnoreCase) >= 0 ||
                                     compareInfo.IndexOf(card.Location, f, CompareOptions.IgnoreCase) >= 0 ||
                                     compareInfo.IndexOf(card.Region, f, CompareOptions.IgnoreCase) >= 0 ||
                                     compareInfo.IndexOf(_t[card.Source.GetDescription()], f, CompareOptions.IgnoreCase) >= 0);
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

        public ViewModelActivator Activator { get; }
    }
}
