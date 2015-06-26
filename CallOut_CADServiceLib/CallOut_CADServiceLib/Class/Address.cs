using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace gCAD.Shared.IntegrationContract
{
    [DataContract]
    public class Address
    {
        [DataMember]
        public string Unit { get; set; }
        [DataMember]
        public string Street { get; set; }
        [DataMember]
        public string City { get; set; }
        [DataMember]
        public string State { get; set; }
        [DataMember]
        public string PostalCode { get; set; }  // ?? required

        //Albert
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Country { get; set; }
    }
}
