using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;


namespace gCAD.Shared.IntegrationContract
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class DispatchedIncident
    {
        /// <summary>
        /// Unique Incident Identifier from CAD
        /// </summary>
        [DataMember]
        public string IncidentNumber { get; set; }

        /// <summary>
        /// Incident Object's Incident Location Address
        /// </summary>
        [DataMember]
        public Address IncidentLocation { get; set; }

        /// <summary>
        /// Selected Incident type
        /// </summary>
        [DataMember]
        public string IncidentType { get; set; }

        /// <summary>
        /// Priority of Incident (Information Only)
        /// </summary>
        [DataMember]
        public string Priority { get; set; }

        /// <summary>
        /// Level of Dispatch expected (Information Only)
        /// </summary>
        [DataMember]
        public int AlarmLevel { get; set; }

        /// <summary>
        /// Date and Time when Resources are dispatched
        /// </summary>
        [DataMember]
        public DateTime DateTime { get; set; }

        /// <summary>
        /// An Incident message could have one or more units dispatched from one or more stations
        /// </summary>
         [DataMember]
        public List<DispatchedUnit> ListOfUnits { get; set; }


        //Albert
         [DataMember]
         public string IncidentTitle { get; set; }

        /*
         * 
         * INCIDENTNO, 
         * 
         * INCIDENT LOCATION
         * 
         * IncidentAddress.StreetName
         * 
         * 
         * 
         * 
         * , INCIDENT TYPE, 
         * 
         * IncidentType.Type
         * 
         * 
         * PRIORITY, 
         * 
         * priority.Code
         * 
         * 
         * ALARM LEVEL,
         * 
         * AlarmLevel.LocalizedDisplayString
         * 
         * DATE/TIME, 
         * 
         * dispatch time.
         * 
         * 
         * (UNIT CALLSIGN, FROM STATUS, UNIT LOCATION, UNIT HOME STATION, UNIT CURRENT STATION)
         * 
         * 
         * 
         * 
         * 
         */
    }
}
