using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;


namespace CallOut_CodingServiceLib.Class
{

    [DataContract]
    public class StationStatus
    {
        public StationStatus(string status, string station, string update)
        {
            Status = status;
            Station = station;
            Update = update;
        }
        [DataMember]
        public string Status { get; set; }
        [DataMember]
        public string Station { get; set; }
        [DataMember]
        public string Update { get; set; }

    }
}
