using ReactiveUI;

namespace GwentTracker.Model
{
    public class Card : ReactiveObject
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public string Flavor { get; set; }
        private int _copies;
        public int Copies
        {
            get => _copies;
            set => this.RaiseAndSetIfChanged(ref _copies, value);
        }
        private bool _obtained;
        public bool Obtained
        {
            get => _obtained;
            set => this.RaiseAndSetIfChanged(ref _obtained, value);
        }
        public string Deck { get; set; }
        public CombatDetails Combat { get; set; }
        public string Type { get; set; }
        public Location[] Locations { get; set; }
    }
}
