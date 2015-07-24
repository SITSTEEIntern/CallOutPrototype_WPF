using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace CallOut_CodingServiceLib.Class
{
    [DataContract]
    public class CodingIncidentMessage
    {

        [DataMember]
        public string CodingID { get; set; }
        [DataMember]
        public string IncidentNo { get; set; }
        [DataMember]
        public string IncidentTitle { get; set; }
        [DataMember]
        public CodingLocation IncidentLocation { get; set; }
        [DataMember]
        public string IncidentType { get; set; }
        [DataMember]
        public int IncidentAlarm { get; set; }
        [DataMember]
        public string IncidentPriority { get; set; }
        [DataMember]
        public DateTime DispatchDateTime { get; set; }
        [DataMember]
        public List<CodingUnits> DispatchUnits { get; set; }
    }
}
