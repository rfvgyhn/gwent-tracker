using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GwentTracker.Model
{
    public class SaveGameInfo
    {
        public IEnumerable<KeyValuePair<int, int>> CardCopies { get; set; }
    }
}
