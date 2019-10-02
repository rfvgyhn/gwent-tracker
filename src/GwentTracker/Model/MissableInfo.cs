using System;

namespace GwentTracker.Model
{
    public class MissableInfo
    {
        private string _cutoff;
        public int[] CardIds { get; set; }
        public string QuestName { get; set; }

        public string Cutoff
        {
            get => _cutoff == "self" ? QuestName : _cutoff;
            set => _cutoff = value;
        }

        public string[] Notes { get; set; }
        public Uri Details { get; set; }
    }
}
