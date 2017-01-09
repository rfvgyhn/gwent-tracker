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
        public ReactiveList<CardViewModel> Cards { get; set; }
        public ReactiveList<Notification> Notifications { get; set; }
        public ReactiveCommand<string, SaveGameInfo> Load { get; set; }
        public ReactiveCommand AddFilter { get; set; }
        public ReactiveCommand RemoveFilter { get; set; }
        public ReactiveList<string> Filters { get; set; }

        ObservableAsPropertyHelper<Visibility> _loaderVisibility;
        public Visibility LoaderVisibility => _loaderVisibility.Value;

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

        public MainWindowViewModel(string saveGamePath)
        {
            Activator = new ViewModelActivator();
            Filters = new ReactiveList<string>();
            Cards = new ReactiveList<CardViewModel>();
            Notifications = new ReactiveList<Notification>();

            this.WhenActivated(d =>
            {
                SaveGamePath = saveGamePath;

                Load = ReactiveCommand.CreateFromTask<string, SaveGameInfo>(LoadSaveGame);
                Load.Subscribe(info =>
                {
                    Model = info;
                    Notifications.Add(new Notification { Name = "Notification 1", Description = "Notification 1 Description" });
                    Notifications.Add(new Notification { Name = "Notification 2", Description = "Notification 2 Description" });
                    ApplyCards(info.Cards);
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
            var files = new[] {"monsters", "neutral", "nilfgaard", "northernrealms", "scoiatael"};
            var cards = new List<Card>();
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

        private void OnRemoveFilter(string filter)
        {
            Filters.Remove(filter);
        }

        private void ApplyCards(IEnumerable<Card> cards)
        {
            var mapped = cards.Select(c => new CardViewModel
                                           {
                                               Index = c.Index,
                                               Copies = c.Copies,
                                               Name = c.Name,
                                               Obtained = c.Obtained,
                                               Deck = c.Deck,
                                               Type = c.Type,
                                               Location = string.Join(", ", c.Locations.Select(l => l.Type == "Base Deck" ? "N/A" : l.Type)),
                                               Region = string.Join(", ", c.Locations.Select(l => l.Type == "Base Deck" ? "N/A" : l.Region))
                                           });
            Cards.Clear();
            foreach (var card in mapped)
                Cards.Add(card);
            //Cards.AddRange(filtered);
        }

        public ViewModelActivator Activator { get; }
    }
}
