using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GwentTracker.Model
{
    [DataContract]
    public class Card
    {
        [DataMember]
        public int? Index { get; set; }
        [DataMember]
        public string Name { get; set; }
        [IgnoreDataMember]
        public int Copies { get; set; }
        [IgnoreDataMember]
        public bool Obtained { get; set; }
    }
}
