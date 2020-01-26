using ReactiveUI;

namespace GwentTracker.Model
{
    public class CollectionProgress : ReactiveObject
    {
        private int _collected;
        public int Collected
        {
            get => _collected;
            set => this.RaiseAndSetIfChanged(ref _collected, value);
        }
        
        private int _needed;
        public int Needed
        {
            get => _needed;
            set => this.RaiseAndSetIfChanged(ref _needed, value);
        }

        private int _copies;

        public int Copies
        {
            get => _copies;
            set => this.RaiseAndSetIfChanged(ref _copies, value);
        }

        private int _total;
        public int Total
        {
            get => _total;
            set => this.RaiseAndSetIfChanged(ref _total, value);
        }
    }
}