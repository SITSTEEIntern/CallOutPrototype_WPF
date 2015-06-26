using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace CallOut_CodingServiceLib.Class
{
    [DataContract]
    public class CodingUnits
    {
        [DataMember]
        public long ID { get; set; }
        [DataMember]
        public string Callsign { get; set; }
        [DataMember]
        public string UnitType { get; set; }
        [DataMember]
        public string FromStatus { get; set; }
        [DataMember]
        public string UnitLocation { get; set; }
        [DataMember]
        public string UnitHomeStation { get; set; }
        [DataMember]
        public string UnitCurrentStation { get; set; }
    }
}
