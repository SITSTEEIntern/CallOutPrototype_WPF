using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel; //for INotify

namespace CallOut_Gateway.Class
{
    public class MessageStatus : INotifyPropertyChanged
    {
        private string _codingID;
        private string _acktimestamp;
        private string _ackfrom;
        private string _ackstatus;
        private string _ackno;
        private string _acktotal;
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
        public string AckTimeStamp
        {
            get { return _acktimestamp; }
            set
            {
                _acktimestamp = value;
                this.NotifyPropertyChanged("AckTimeStamp");
            }
        }
        public string AckFrom
        {
            get { return _ackfrom; }
            set
            {
                _ackfrom = value;
                this.NotifyPropertyChanged("AckFrom");
            }
        }
        public string AckStatus
        {
            get { return _ackstatus; }
            set
            {
                _ackstatus = value;
                this.NotifyPropertyChanged("AckStatus");
            }
        }
        public string AckNo
        {
            get { return _ackno; }
            set
            {
                _ackno = value;
                this.NotifyPropertyChanged("AckNo");
            }
        }
        public string AckTotal
        {
            get { return _acktotal; }
            set
            {
                _acktotal = value;
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
