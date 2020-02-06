using System;
using System.Collections.Generic;
using System.ComponentModel;
using ReactiveUI;

namespace GwentTracker.Model
{
    [Flags]
    public enum CardSource
    {
        [Description("Base Game")]
        BaseGame = 1,
        
        [Description("Hearts of Stone")]
        HeartsOfStone = 2,
        
        [Description("Blood and Wine")]
        BloodAndWine = 4,
        
        [Description("DLC")]
        Dlc = HeartsOfStone | BloodAndWine
    }
    
    public class Card : ReactiveObject
    {
        private static readonly HashSet<int> HoSCards = new HashSet<int> { 17, 18, 19, 20, 21, 368, 478, 1005, 2005, 3005, 4005 };
        private static readonly HashSet<int> BandWCards = new HashSet<int> { 22, 23 };
        public int Index { get; set; }
        public string Name { get; set; }
        public string Flavor { get; set; }
        public int MaxCopies { get; set; } = 1;
        public int? AttachedTo { get; set; }

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
                if (HoSCards.Contains(Index))
                    return CardSource.HeartsOfStone;

                if ((Index >= 500 && Index < 600) || Index > 5000 || BandWCards.Contains(Index))
                    return CardSource.BloodAndWine;

                return CardSource.BaseGame;                
            }
        }
    }
}
