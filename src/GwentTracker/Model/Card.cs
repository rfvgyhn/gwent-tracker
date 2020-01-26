using ReactiveUI;

namespace GwentTracker.Model
{
    public enum CardSource
    {
        BaseGame,
        Dlc
    }
    
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

        public CardSource Source
        {
            get
            {
                if (Index == 4005 || Index == 2005 || Index == 1005 || Index == 3005 ||
                    (Index >= 500 && Index < 600) || Index > 5000)
                {
                    return CardSource.Dlc;
                }

                return CardSource.BaseGame;                
            }
        }
    }
}
