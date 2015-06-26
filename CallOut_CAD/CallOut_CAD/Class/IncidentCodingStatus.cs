using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel; //for INotify

namespace CallOut_CAD.Class
{
    public class IncidentCodingStatus : INotifyPropertyChanged
    {
        private string _codingID;
        private string _trackStation;
        private string _trackStatus;
        private string _trackUnit;
        private string _ackFrom;
        private string _ackStatus;
        private string _ackTimeStamp;
        private string _ackNo;
        private string _ackTotal;
        public event PropertyChangedEventHandler PropertyChanged;

        public string CodingID
        {
            get { return _codingID; }
            set
            {
                _codingID = value;
                this.NotifyPropertyChanged("CodingID");
            }
        }
        public string TrackStation
        {
            get { return _trackStation; }
            set
            {
                _trackStation = value;
                this.NotifyPropertyChanged("TrackStation");
            }
        }
        public string TrackStatus
        {
            get { return _trackStatus; }
            set
            {
                _trackStatus = value;
                this.NotifyPropertyChanged("TrackStatus");
            }
        }
        public string TrackUnit
        {
            get { return _trackUnit; }
            set
            {
                _trackUnit = value;
                this.NotifyPropertyChanged("TrackUnit");
            }
        }
        public string AckFrom
        {
            get { return _ackFrom; }
            set
            {
                _ackFrom = value;
                this.NotifyPropertyChanged("AckFrom");
            }
        }
        public string AckStatus
        {
            get { return _ackStatus; }
            set
            {
                _ackStatus = value;
                this.NotifyPropertyChanged("AckStatus");
            }
        }
        public string AckTimeStamp
        {
            get { return _ackTimeStamp; }
            set
            {
                _ackTimeStamp = value;
                this.NotifyPropertyChanged("AckTimeStamp");
            }
        }

        public string AckNo
        {
            get { return _ackNo; }
            set
            {
                _ackNo = value;
                this.NotifyPropertyChanged("AckNo");
            }
        }

        public string AckTotal
        {
            get { return _ackTotal; }
            set
            {
                _ackTotal = value;
                this.NotifyPropertyChanged("AckTotal");
            }
        }

        private void NotifyPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
    }
}
