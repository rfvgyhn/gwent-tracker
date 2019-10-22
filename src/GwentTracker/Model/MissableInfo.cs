using System;
using System.Reactive.Linq;
using Avalonia.Media;
using ReactiveUI;

namespace GwentTracker.Model
{
    public enum MissableState
    {
        Active,
        Missed,
        Obtained
    }
    
    public class MissableInfo : ReactiveObject
    {
        public int[] CardIds { get; set; }
        public string QuestName { get; set; }
        public string[] Notes { get; set; }
        public Uri Details { get; set; }
        public string Region { get; set; }

        private MissableState _state = MissableState.Active;
        public MissableState State
        {
            get => _state;
            set => this.RaiseAndSetIfChanged(ref _state, value);
        }
        
        private string _cutoff;
        public string Cutoff
        {
            get => _cutoff == "self" ? QuestName : _cutoff;
            set => _cutoff = value;
        }
        
        private readonly ObservableAsPropertyHelper<bool> _detailsVisible;
        public bool DetailsVisible => _detailsVisible.Value;
        
        private readonly ObservableAsPropertyHelper<TextWrapping> _textWrapping;
        public TextWrapping TextWrap => _textWrapping.Value;
        
        private readonly ObservableAsPropertyHelper<IBrush> _color;
        public IBrush Color => _color.Value;

        public MissableInfo()
        {
            _detailsVisible = this.WhenAnyValue(x => x.State)
                                  .Select(s => s == MissableState.Active)
                                  .ToProperty(this, x => x.DetailsVisible);
            _textWrapping = this.WhenAnyValue(x => x.State)
                                .Select(s => s == MissableState.Active ? TextWrapping.Wrap : TextWrapping.NoWrap)
                                .ToProperty(this, x => x.TextWrap);
            _color = this.WhenAnyValue(x => x.State)
                         .Select(s =>
                         {
                             switch (s)
                             {
                                 case MissableState.Active:
                                     return Brushes.Blue;
                                 case MissableState.Missed:
                                     return Brushes.Red;
                                 case MissableState.Obtained:
                                     return Brushes.Green;
                                 default:
                                     throw new ArgumentOutOfRangeException(nameof(s), s, null);
                             }
                         })
                         .ToProperty(this, x => x.Color);
        }
    }
}
