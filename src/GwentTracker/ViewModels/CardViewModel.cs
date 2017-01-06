using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GwentTracker.ViewModels
{
    public class CardViewModel
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public int Copies { get; set; }
        public bool Obtained { get; set; }
    }
}
