using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace CallOut_ConsoleWPF.Class
{
    public class UnitsStatus : INotifyPropertyChanged
    {
        private string _callsign;
        private string _unittype;
        public event PropertyChangedEventHandler PropertyChanged;

        public string CallSign
        {
            get { return _callsign; }
            set
            {
                _callsign = value;
                this.NotifyPropertyChanged("CallSign");
            }
        }
        public string UnitType
        {
            get { return _unittype; }
            set
            {
                _unittype = value;
                this.NotifyPropertyChanged("UnitType");
            }
        }

        private void NotifyPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
    }
}
