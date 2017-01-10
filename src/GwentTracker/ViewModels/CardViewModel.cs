using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GwentTracker.Model;

namespace GwentTracker.ViewModels
{
    public class CardViewModel
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public int Copies { get; set; }
        public bool Obtained { get; set; }
        public string Deck { get; set; }
        public string Type { get; set; }
        public string Location => string.Join(", ", Locations.Select(l => l.Type == "Base Deck" ? "N/A" : l.Type));
        public string Region => string.Join(", ", Locations.Select(l => l.Type == "Base Deck" ? "N/A" : l.Region));
        public CombatDetails Combat { get; set; }
        public Location[] Locations { get; set; }
    }
}
