using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace CallOut_ConsoleWPF.Class
{
    public class ConsoleLog : INotifyPropertyChanged
    {
        private string _codingID;
        private string _acktimestamp;
        private string _ackfrom;
        private string _ackstatus;
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

        private void NotifyPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
    }
}
