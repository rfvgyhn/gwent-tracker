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
        private readonly string _textureStringFormat;

        public CardViewModel(string textureStringFormat)
        {
            _textureStringFormat = textureStringFormat;
        }

        public int Index { get; set; }
        public string Name { get; set; }
        public int Copies { get; set; }
        public bool Obtained { get; set; }
        public string Deck { get; set; }
        public string Type { get; set; }
        public string Texture => string.Format(_textureStringFormat, Index);
        public string Location => string.Join(", ", Locations.Select(l => l.Type).Distinct());
        public string Region => string.Join(", ", Locations.Select(l => l.Type == "Base Deck" ? l.Type : l.Region).Distinct());
        public CombatDetails Combat { get; set; }
        public Location[] Locations { get; set; }
    }
}
