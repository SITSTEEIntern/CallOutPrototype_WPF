using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel; //for INotify

namespace CallOut_Gateway.Class
{
    public class CodingStatus : INotifyPropertyChanged
    {
        private string _incidentID;
        private string _codingID;
        private string _received;
        private string _updated;
        private string _pending;
        private string _ack;
        private string _rejected;
        private string _failed;
        public event PropertyChangedEventHandler PropertyChanged;

        public string IncidentID
        {
            get { return _incidentID; }
            set
            {
                _incidentID = value;
                this.NotifyPropertyChanged("IncidentID");
            }
        }
        public string CodingID
        {
            get { return _codingID; }
            set
            {
                _codingID = value;
                this.NotifyPropertyChanged("CodingID");
            }
        }
        public string Received
        {
            get { return _received; }
            set
            {
                _received = value;
                this.NotifyPropertyChanged("Received");
            }
        }
        public string Updated
        {
            get { return _updated; }
            set
            {
                _updated = value;
                this.NotifyPropertyChanged("Updated");
            }
        }
        public string Pending
        {
            get { return _pending; }
            set
            {
                _pending = value;
                this.NotifyPropertyChanged("Pending");
            }
        }
        public string Acknowledged
        {
            get { return _ack; }
            set
            {
                _ack = value;
                this.NotifyPropertyChanged("Acknowledged");
            }
        }
        public string Rejected
        {
            get { return _rejected; }
            set
            {
                _rejected = value;
                this.NotifyPropertyChanged("Rejected");
            }
        }
        public string Failed
        {
            get { return _failed; }
            set
            {
                _failed = value;
                this.NotifyPropertyChanged("Failed");
            }
        }

        private void NotifyPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
    }
}
