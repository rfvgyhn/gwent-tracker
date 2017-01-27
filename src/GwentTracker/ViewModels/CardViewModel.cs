using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GwentTracker.Model;
using ReactiveUI;

namespace GwentTracker.ViewModels
{
    public class CardViewModel : ReactiveObject
    {
        private readonly string _textureStringFormat;

        public CardViewModel(string textureStringFormat)
        {
            _textureStringFormat = textureStringFormat;
        }

        public int Index { get; set; }
        public string Name { get; set; }
        public string Flavor { get; set; }
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
        private bool _isVisible;
        public bool IsHidden
        {
            get { return _isVisible; }
            set { this.RaiseAndSetIfChanged(ref _isVisible, value); }
        }
        public string Deck { get; set; }
        public string Type { get; set; }
        public string Texture => string.Format(_textureStringFormat, Index);
        public string Location => string.Join(", ", Locations.Select(l => l.Type).Distinct());
        public string Region => string.Join(", ", Locations.Select(l => l.Type == "Base Deck" ? l.Type : l.Region).Distinct());
        public CombatDetails Combat { get; set; }
        public Location[] Locations { get; set; }
    }
}
