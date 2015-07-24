using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;


namespace CallOut_CodingServiceLib.Class
{
    [DataContract]
    public class CodingAckMessage
    {
        [DataMember]
        public string ConsoleID { get; set; }
        [DataMember]
        public string IncidentNo { get; set; }
        [DataMember]
        public string CodingID { get; set; }
        [DataMember]
        public string AckStatus { get; set; }
        [DataMember]
        public string AckTimeStamp { get; set; }
        [DataMember]
        public List<string> AckUnits { get; set; }
    }
}
