using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using CallOut_Gateway.ServiceReference1;

namespace CallOut_Gateway.Class
{
    [DataContract]
    public class GatewayTracker
    {
        [DataMember]
        public string CodingID { get; set; }
        [DataMember]
        public string IncidentID { get; set; }
        [DataMember]
        public List<CodingUnits> DispatchUnits { get; set; }
        [DataMember]
        public Dictionary<string, string> StationStatus { get; set; }
    }
}
