using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace CallOut_CADServiceLib.Class
{
    [DataContract]
    public class CADIncidentAck
    {
        [DataMember]
        public string CodingID { get; set; }
        [DataMember]
        public List<Tracking> AckTracking { get; set; }
        [DataMember]
        public DateTime AckTimeStamp { get; set; }
        [DataMember]
        public int AckNo { get; set; }
        [DataMember]
        public int AckTotal { get; set; }
    }
}
