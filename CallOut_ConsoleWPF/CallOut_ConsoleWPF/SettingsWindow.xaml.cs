//using System;
using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
//using System.Windows.Input;
//using System.Windows.Media;
//using System.Windows.Media.Imaging;
//using System.Windows.Shapes;
using System.Threading;
using System.ComponentModel;
using System.ServiceModel;
using System.Diagnostics; //for debug

// Location of the proxy.
using CallOut_ConsoleWPF.ServiceReference1;

//Class
using CallOut_ConsoleWPF.Class;

namespace CallOut_ConsoleWPF
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    // Specify for the callback to NOT use the current synchronization context
    [CallbackBehavior(
        ConcurrencyMode = ConcurrencyMode.Single,
        UseSynchronizationContext = false)]
    public partial class SettingsWindow : Window, ServiceReference1.CallOut_CodingServiceCallback
    {
        //Declaration
        private SynchronizationContext _uiSyncContext = null;
        private ServiceReference1.CallOut_CodingServiceClient _CallOut_CodingService = null;

        private List<ConsoleLog> _ConsoleLogList = new List<ConsoleLog>();

        //For sort at column header
        private GridViewColumnHeader listViewSortCol = null;
        private SortAdorner listViewSortAdorner = null;

        public SettingsWindow()
        {
            InitializeComponent();
            //Sub window load and closing eventhandler
            Loaded += MyWindow_Loaded;
            Closing += MyWindow_Closing;
        }

        private void MyWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Capture the UI synchronization context
            _uiSyncContext = SynchronizationContext.Current;

            // The client callback interface must be hosted for the server to invoke the callback
            // Open a connection to the message service via the proxy (qualifier ServiceReference1 needed due to name clash)
            _CallOut_CodingService = new ServiceReference1.CallOut_CodingServiceClient(new InstanceContext(this), "WSDualHttpBinding_CallOut_CodingService");
            _CallOut_CodingService.Open();

            //Fill up the combobox items
            foreach (StationStatus station in _CallOut_CodingService.GetStationStatus())
            {
                this.comboID.Items.Add(station.Station);
            }

            //Fill the combobox selected text
            this.comboID.Text = Properties.Settings.Default.CurrentID;

            //DataGrid Bind
            _ConsoleLogList.Clear();

            //Load out the console log status from configfile
            if (this.comboID.Text != "")
            {
                this.comboID.IsEnabled = false;

                if (Properties.Settings.Default.ConsoleLogList != null)
                {
                    foreach (string log in Properties.Settings.Default.ConsoleLogList)
                    {
                        string[] parts = log.Split(',');
                        //Check is it corresponding stationID
                        if (parts[0] == this.comboID.Text)
                        {
                            ConsoleLog consolelog = new ConsoleLog();
                            consolelog.CodingID = parts[1];
                            consolelog.AckTimeStamp = parts[2];
                            consolelog.AckFrom = parts[3];
                            consolelog.AckStatus = parts[4];
                            _ConsoleLogList.Add(consolelog);
                        }
                    }
                }
            }

            this.lvConsoleLog.ItemsSource = _ConsoleLogList;
        }

        private void MyWindow_Closing(object sender, CancelEventArgs e)
        {
            //possible of remove the stationID in the setting file?

            //Terminate the connection to the service.
            _CallOut_CodingService.Close();

        }

        private void btnSet_Click(object sender, RoutedEventArgs e)
        {
            //Disable the combobox
            this.comboID.IsEnabled = false;
            //Set the stationID chosen to "currentstation" in the setting 
            Properties.Settings.Default.CurrentID = this.comboID.Text;
            Properties.Settings.Default.Save();

            if (Properties.Settings.Default.ConsoleLogList != null)
            {
                foreach (string log in Properties.Settings.Default.ConsoleLogList)
                {
                    string[] parts = log.Split(',');
                    //Check is it corresponding stationID
                    if (parts[0] == this.comboID.Text)
                    {
                        ConsoleLog consolelog = new ConsoleLog();
                        consolelog.CodingID = parts[1];
                        consolelog.AckTimeStamp = parts[2];
                        consolelog.AckFrom = parts[3];
                        consolelog.AckStatus = parts[4];
                        _ConsoleLogList.Add(consolelog);
                    }
                }
            }
            this.lvConsoleLog.ItemsSource = null;
            this.lvConsoleLog.ItemsSource = _ConsoleLogList;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            //Enable the combobox to allow user to change station
            this.comboID.IsEnabled = true;
            Properties.Settings.Default.CurrentID = "";
            Properties.Settings.Default.Save();
            //Clear the datagrid
            _ConsoleLogList.Clear();
            //this.lvConsoleLog.Items.Refresh();
            //Had to use this instead of refresh as the columnh header sort overlay on the itemsource
            this.lvConsoleLog.ItemsSource = null;
            this.lvConsoleLog.ItemsSource = _ConsoleLogList;
        }

        private void lvConsoleLogColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader column = (sender as GridViewColumnHeader);
            string sortBy = column.Tag.ToString();
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(lvConsoleLog.ItemsSource);

            if (listViewSortCol != null)
            {
                AdornerLayer.GetAdornerLayer(listViewSortCol).Remove(listViewSortAdorner);
                lvConsoleLog.Items.SortDescriptions.Clear();
            }

            ListSortDirection newDir = ListSortDirection.Ascending;
            if (listViewSortCol == column && listViewSortAdorner.Direction == newDir)
            {
                newDir = ListSortDirection.Descending;
            }
            listViewSortCol = column;
            listViewSortAdorner = new SortAdorner(listViewSortCol, newDir);
            AdornerLayer.GetAdornerLayer(listViewSortCol).Add(listViewSortAdorner);
            //Sort and the arrow icon
            view.SortDescriptions.Add(new SortDescription(sortBy, newDir));
            lvConsoleLog.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
        }

        #region CallOut_CodingServiceCallback Methods

        public void ConsoleDisplayMsg(CodingIncidentMessage codingIncidentMsg)
        {}

        public void ConsoleRcvConnStatus()
        {}

        #region Methods not for Console

        public void EditConnStatus(string Name, string Status)
        { }
        public void RcvCodingAckMsg(CodingAckMessage codingAckMsg)
        { }
        public void NotifyConsoleNotConnected(string userName, CodingIncidentMessage codingIncidentMsg)
        { }
        public void GatewayRcvConnStatus(string station)
        { }
        public void UpdateRemoveMsgList(string CodingID)
        { }
        public void StartTargetTimeoutTimer(string console, CodingIncidentMessage codingIncidentMsg) 
        { }

        #endregion

        #endregion
    }
}
