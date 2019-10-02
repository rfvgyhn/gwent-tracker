using System;

namespace GwentTracker.Model
{
    public class MissableInfo
    {
        public int[] CardIds { get; set; }
        public string QuestName { get; set; }
        public string[] Notes { get; set; }
        public Uri Details { get; set; }
        public string Region { get; set; }
        
        private string _cutoff;
        public string Cutoff
        {
            get => _cutoff == "self" ? QuestName : _cutoff;
            set => _cutoff = value;
        }
    }
}
