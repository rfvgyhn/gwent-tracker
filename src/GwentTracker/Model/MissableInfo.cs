using System;

namespace GwentTracker.Model
{
    public class MissableInfo
    {
        public int[] CardIds { get; set; }
        public string QuestName { get; set; }
        public string Cutoff { get; set; }
        public string[] Notes { get; set; }
        public Uri Details { get; set; }
    }
}
