using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GwentTracker.Model
{
    public class Card : ReactiveObject
    {
        public int Index { get; set; }
        public string Name { get; set; }
        private int _copies;
        public int Copies
        {
            get { return _copies; }
            set { this.RaiseAndSetIfChanged(ref _copies, value); }
        }
        private bool _obtained;
        public bool Obtained
        {
            get { return _obtained; }
            set { this.RaiseAndSetIfChanged(ref _obtained, value); }
        }
        public string Deck { get; set; }
        public CombatDetails Combat { get; set; }
        public string Type { get; set; }
        public Location[] Locations { get; set; }
    }
}
