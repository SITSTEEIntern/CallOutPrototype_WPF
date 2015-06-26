using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace CallOut_CADServiceLib.Class
{
    [DataContract]
    public class Tracking
    {
        [DataMember]
        public string Station { get; set; }
        [DataMember]
        public string Status { get; set; }
        [DataMember]
        public List<string> Unit { get; set; }
    }
}
