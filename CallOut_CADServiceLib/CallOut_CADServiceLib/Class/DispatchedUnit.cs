using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;


namespace gCAD.Shared.IntegrationContract
{
    /// <summary>
    /// Dispatched Unit Entity Infomrations
    /// </summary>
    [DataContract]
    public class DispatchedUnit
    {
        /// <summary>
        /// Unique Identifier
        /// </summary>
        [DataMember]
        public long ID { get; set; }

        /// <summary>
        /// Unit Call Sign
        /// </summary>
        [DataMember]
        public string CallSign { get; set; }

        /// <summary>
        /// From Status
        /// </summary>
        [DataMember]
        public string Status { get; set; }

        /// <summary>
        /// Unit Location
        /// </summary>
        [DataMember]
        public string Location { get; set; }

        /// <summary>
        /// Unit Home Station
        /// </summary>
        [DataMember]
        public string HomeStation { get; set; }

        /// <summary>
        /// Unit Current Station
        /// </summary>
        [DataMember]
        public string CurrentStation { get; set; }

        //Albert
        [DataMember]
        public string UnitType { get; set; }

        /*
        ///
        /// Long ID
        /// 
        /// UNIT CALLSIGN
        ///  Callsign.Name
        /// 
        /// , FROM STATUS, 
        ///  status.LocalizedDisplayString
        /// 
        /// 
        /// UNIT LOCATION,
        /// 
        /// Location.displayAddress
        /// 
        ///  UNIT HOME STATION
        /// HomeStation.name
        /// 
        /// , UNIT CURRENT STATION
        /// 
        /// CurrentStation.name
        /// */
    }
}
