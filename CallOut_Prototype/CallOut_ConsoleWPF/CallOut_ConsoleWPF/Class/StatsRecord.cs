using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOut_ConsoleWPF.Class
{
    class StatsRecord
    {
        private string _ConsoleName;
        private int _IncidentCount;
        private int _AckCount;
        private int _RejectedCount;
        private int _FailedCount;

        public StatsRecord(string _consolename, int incidentcount, int ackcount, int rejectedcount, int failedcount) 
        {
            _ConsoleName = _consolename;
            _IncidentCount = incidentcount;
            _AckCount = ackcount;
            _RejectedCount = rejectedcount;
            _FailedCount = failedcount;
        }

        public string ConsoleName
        {
            get { return _ConsoleName; }
            set { _ConsoleName = value;}
        }

        public int IncidentCount 
        {
            get { return _IncidentCount; }
            set { _IncidentCount = value; }
        }

        public int AckCount
        {
            get { return _AckCount; }
            set { _AckCount = value; }
        }

        public int RejectedCount
        {
            get { return _RejectedCount; }
            set { _RejectedCount = value; }
        }

        public int FailedCount
        {
            get { return _FailedCount; }
            set { _FailedCount = value; }
        }
    }
}
