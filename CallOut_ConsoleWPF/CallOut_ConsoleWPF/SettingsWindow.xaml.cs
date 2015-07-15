using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Threading;
using System.ComponentModel;
using System.ServiceModel;
using System.Diagnostics; //for debug
using System.Net.NetworkInformation;

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

        private bool _isCodingSvcIPSet = false;
        private bool _isStationIDSet = false;

        //Internet connection
        private bool isInternetup = true; 

        public SettingsWindow()
        {
            InitializeComponent();
            //Sub window load and closing eventhandler
            Loaded += MyWindow_Loaded;
            Closing += MyWindow_Closing;
            NetworkChange.NetworkAvailabilityChanged +=
                new NetworkAvailabilityChangedEventHandler(DoNetworkChanged);

            //Check for internet connection first
            if (CheckForInternetConnection())
            {
                isInternetup = true;
            }
            else
            {
                isInternetup = false;
                ShowMessageBox("Please check the internet connection to continue");
            }
        }

        private void MyWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Capture the UI synchronization context
            _uiSyncContext = SynchronizationContext.Current;

            //Check whether CodingIP exist
            if (Properties.Settings.Default.CodingIP != null && Properties.Settings.Default.CodingIP != "")
            {
                this.txtCodingSvcIP.Text = Properties.Settings.Default.CodingIP;
                this.txtCodingSvcIP.IsEnabled = false;

                _isCodingSvcIPSet = true;
                this.btnSetIP.Content = "Unset IP";

                string endpointaddress = "net.tcp://" + this.txtCodingSvcIP.Text.Trim() + ":8000/CallOut_CodingService/service";
                EndpointAddress endpointaddr = new EndpointAddress(new Uri(endpointaddress));
                _CallOut_CodingService = new ServiceReference1.CallOut_CodingServiceClient(new InstanceContext(this), "NetTcpBinding_CallOut_CodingService", endpointaddr);
                _CallOut_CodingService.Open();

                //Connection is UP!!!

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
                    _isStationIDSet = true;
                    this.btnSetID.Content = "Unset";

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
        }

        private void MyWindow_Closing(object sender, CancelEventArgs e)
        {
            //possible of remove the stationID in the setting file?
            if (isInternetup)
            {
                //Cut off the proxy / channel
                if (_CallOut_CodingService != null)
                {
                    _CallOut_CodingService.Close();
                }
            }
            else 
            {
                //Cut off the proxy / channel
                if (_CallOut_CodingService != null)
                {
                    //Use abort as internet is been cut
                    _CallOut_CodingService.Abort();
                }
            }

            //remove the networkstatus handler
            NetworkChange.NetworkAvailabilityChanged -=
                new NetworkAvailabilityChangedEventHandler(DoNetworkChanged);
        }

        #region NETWORK

        /// <summary>
        /// Event handler used to capture availability changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        void DoNetworkChanged(object sender, NetworkAvailabilityEventArgs e)
        {
            ReportAvailability();
        }

        public static bool CheckForInternetConnection()
        {
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                try
                {
                    Ping myPing = new Ping();
                    String host = "8.8.8.8"; //google DNS (if one is not enought can ping together stable website)
                    byte[] buffer = new byte[32];
                    int timeout = 1000;
                    PingOptions pingOptions = new PingOptions();
                    PingReply reply = myPing.Send(host, timeout, buffer, pingOptions);
                    return (reply.Status == IPStatus.Success);
                }
                catch (Exception)
                {
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// Report the current network availability.
        /// </summary>

        private void ReportAvailability()
        {
            if (CheckForInternetConnection())
            {
                isInternetup = true;
                //Do not have to open another messagebox or proxy as mainwindow had been triggered
                if (Properties.Settings.Default.CodingIP != null && Properties.Settings.Default.CodingIP != "")
                {
                    string endpointaddress = "net.tcp://" + Properties.Settings.Default.CodingIP + ":8000/CallOut_CodingService/service";
                    EndpointAddress endpointaddr = new EndpointAddress(new Uri(endpointaddress));
                    _CallOut_CodingService = new ServiceReference1.CallOut_CodingServiceClient(new InstanceContext(this), "NetTcpBinding_CallOut_CodingService", endpointaddr);
                    _CallOut_CodingService.Open();
                }
            }
            else
            {
                isInternetup = false;

                if (Properties.Settings.Default.CodingIP != null && Properties.Settings.Default.CodingIP != "")
                {
                    _CallOut_CodingService.Abort();
                    _CallOut_CodingService = null;
                }
            }
        }

        public void ShowMessageBox(string msg)
        {
            var thread = new Thread(() =>
            {
                MessageBox.Show(msg);
            });
            thread.Start();
        }

        #endregion

        private void btnSetIP_Click(object sender, RoutedEventArgs e)
        {
            if (isInternetup)
            {
                if (_isCodingSvcIPSet)
                {
                    //UnSetIP

                    //Clear ComboBox and Console Log
                    this.comboID.Items.Clear();

                    _ConsoleLogList.Clear();
                    this.lvConsoleLog.ItemsSource = _ConsoleLogList;

                    //Cut off the proxy / channel
                    _CallOut_CodingService.Close();
                    _CallOut_CodingService = null;

                    //Remove the IP Address in config file
                    Properties.Settings.Default.CodingIP = "";
                    Properties.Settings.Default.Save();

                    this.txtCodingSvcIP.IsEnabled = true;
                    _isCodingSvcIPSet = false;
                    this.btnSetIP.Content = "Set IP";
                }
                else
                {
                    //Set IP

                    //Trigger to create Coding Service Client with the input IP
                    try
                    {
                        string endpointaddress = "net.tcp://" + this.txtCodingSvcIP.Text.Trim() + ":8000/CallOut_CodingService/service";
                        _CallOut_CodingService = new ServiceReference1.CallOut_CodingServiceClient(new InstanceContext(this), "NetTcpBinding_CallOut_CodingService", endpointaddress);
                        _CallOut_CodingService.Open();

                        //Save the IP address that is confirm
                        Properties.Settings.Default.CodingIP = this.txtCodingSvcIP.Text.Trim();
                        Properties.Settings.Default.Save();

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

                        this.txtCodingSvcIP.IsEnabled = false;
                        _isCodingSvcIPSet = true;
                        this.btnSetIP.Content = "Unset IP";
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show("Invalid IP address...");
                    }
                }
            }
            else 
            {
                ShowMessageBox("Please check the internet connection to continue");
            }
        }

        private void btnSetID_Click(object sender, RoutedEventArgs e)
        {
            if (isInternetup)
            {
                if (_isStationIDSet)
                {
                    //unset
                    //Enable the combobox to allow user to change station
                    this.comboID.IsEnabled = true;
                    _isStationIDSet = false;
                    this.btnSetID.Content = "Set";
                    Properties.Settings.Default.CurrentID = "";
                    Properties.Settings.Default.Save();
                    //Clear the datagrid
                    _ConsoleLogList.Clear();
                    //this.lvConsoleLog.Items.Refresh();
                    //Had to use this instead of refresh as the columnh header sort overlay on the itemsource
                    this.lvConsoleLog.ItemsSource = null;
                    this.lvConsoleLog.ItemsSource = _ConsoleLogList;
                }
                else
                {
                    //set
                    //Disable the combobox
                    this.comboID.IsEnabled = false;
                    _isStationIDSet = true;
                    this.btnSetID.Content = "Unset";
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
            }
            else
            {
                ShowMessageBox("Please check the internet connection to continue");
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (isInternetup)
            {
                this.Close();
            }
            else
            {
                ShowMessageBox("Please check the internet connection to continue");
            }
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
