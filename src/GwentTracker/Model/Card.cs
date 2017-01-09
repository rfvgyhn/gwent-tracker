using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GwentTracker.Model
{
    public class Card
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public int Copies { get; set; }
        public bool Obtained { get; set; }
        public string Deck { get; set; }
        public CombatDetails Combat { get; set; }
        public string Type { get; set; }
        public Location[] Locations { get; set; }
    }
}
