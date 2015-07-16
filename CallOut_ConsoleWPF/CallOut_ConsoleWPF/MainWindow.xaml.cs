using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.ServiceModel;
using System.ComponentModel; //for bindinglist, inotify
using System.Diagnostics; //for debug
using System.Net;
using System.Net.NetworkInformation;

using System.Timers; //Timer
using System.Text;

using NAudio.Wave;
using System.IO;
using System.Speech;
using System.Speech.Synthesis;

// Location of the proxy.
using CallOut_ConsoleWPF.ServiceReference1;
using System.Collections.ObjectModel;

//Class
using CallOut_ConsoleWPF.Class;

namespace CallOut_ConsoleWPF
{
    // Specify for the callback to NOT use the current synchronization context
    [CallbackBehavior(
        ConcurrencyMode = ConcurrencyMode.Single,
        UseSynchronizationContext = false)]
    public partial class MainWindow : Window, ServiceReference1.CallOut_CodingServiceCallback
    {
        //Declaration
        private SynchronizationContext _uiSyncContext = null;
        private ServiceReference1.CallOut_CodingServiceClient _CallOut_CodingService = null;

        //For toggle of button
        private bool _isConnected = false;

        //Quene of recevied incident message
        private Queue<CodingIncidentMessage> IncidentMsgQueue = new Queue<CodingIncidentMessage>();
        //List that hold the message codingID that had already failed/timeout in gateway
        private List<string> FailedCodingID = new List<string>();

        //For list to datagirdview
        private List<UnitsStatus> _UnitsStatusList = new List<UnitsStatus>();

        //Holding current codingID (May had potential issue of overwrite need to change later)
        private string _currIncidentNo = "";
        private string _currCodingID = "";

        //For sort at column header
        private GridViewColumnHeader listViewSortCol = null;
        private SortAdorner listViewSortAdorner = null;

        //Variable for Text to speech
        private const string URL = "http://translate.google.com/translate_tts?tl={0}&q={1}";
        private bool isMsgAlarmPlayed = false;
        private string speechsentence = "";
        private static WaveOut waveOut = new WaveOut();
        private static SpeechSynthesizer speechSynthesizerObj;

        //number count for title display
        private bool isfirstdisplay = true;
        private int countdisplay = 1;
        private int counttotal = 0;

        //Hidden string
        private string lblCodingID = "";
        private string lblStatus = "";

        //Internet connection
        private bool isInternetup = true;
        private bool beenCutOffBefore = false;

        //For stats counting purpose
        
        private int IncidentCount = 0;
        private int AckCount = 0;
        private int RejectedCount = 0;
        private int FailedCount = 0;
        private List<StatsRecord> StatsRecordList = new List<StatsRecord>();


        public MainWindow()
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

            //Clear the IP and ID
            if (Properties.Settings.Default.CodingIP != "" && Properties.Settings.Default.CodingIP != null)
            {
                Properties.Settings.Default.CodingIP = "";
                Properties.Settings.Default.CurrentID = "";
                Properties.Settings.Default.Save();
            }

            //DataGrid Bind
            _UnitsStatusList.Clear();
            this.lvUnits.ItemsSource = _UnitsStatusList;

        }

        private void MyWindow_Closing(object sender, CancelEventArgs e)
        {
            //Terminate the connection to the service.
            //if (_isConnected)
            //{
            //    _CallOut_CodingService.Close();
            //}

            //Empty the IP address when closing alway is ""?
            if (Properties.Settings.Default.CodingIP != "" && Properties.Settings.Default.CodingIP != null)
            {
                Properties.Settings.Default.CodingIP = "";
                Properties.Settings.Default.CurrentID = "";
                Properties.Settings.Default.Save();
                if (isInternetup)
                {
                    _CallOut_CodingService.Close();
                }
                else 
                {
                    _CallOut_CodingService.Abort();
                }
                
            }

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
                if (IncidentMsgQueue.Count == 0)
                {
                    beenCutOffBefore = false;
                }
                //open another thread to show message box so it wont hold up the rest of the code
                ShowMessageBox("Internet connection is up...");
                //establish new
                //Or check one of the config file variable for IP address or null (check for codingservice null to determine is offline at setting or main window)
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
                beenCutOffBefore = true;
                //open another thread to show message box so it wont hold up the rest of the code
                ShowMessageBox("Internet connection is down...");
                //abort only closes the client. The service instance/session is still alive until instance timeout.
                if (Properties.Settings.Default.CodingIP != null && Properties.Settings.Default.CodingIP != "" && _CallOut_CodingService!= null)
                {
                    _CallOut_CodingService.Abort();
                    //_CallOut_CodingService = null;
                }
                //Simulate Logout from console ID
                SendOrPostCallback callback =
                    delegate(object state)
                    {
                        if (counttotal != 0)
                        {
                            this.Title = "Call Out Console (" + countdisplay.ToString() + " of " + counttotal.ToString() + ")";
                        }
                        _isConnected = false;
                        this.btnConnect.IsChecked = false;
                        //Update hidden label
                        lblStatus = "";

                        //bool isNewStatsRecord = true; 
                        //foreach (StatsRecord record in StatsRecordList)
                        //{
                        //    if (record.ConsoleName.Equals(Properties.Settings.Default.CurrentID))
                        //    {
                        //        isNewStatsRecord = false;
                        //        break;
                        //    }
                        //}
                        //if (isNewStatsRecord)
                        //{
                        //    StatsRecord newstatsrecord = new StatsRecord(Properties.Settings.Default.CurrentID, IncidentCount, AckCount, RejectedCount, FailedCount);
                        //    StatsRecordList.Add(newstatsrecord);
                        //}
                        IncidentCount = 0;
                        AckCount = 0;
                        RejectedCount = 0;
                        FailedCount = 0;
                    };

                _uiSyncContext.Post(callback, "Internet Down");

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

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            if (isInternetup)
            {
                SettingsWindow winsetting = new SettingsWindow();
                winsetting.ShowDialog();

                if (isInternetup)
                {
                    if (_CallOut_CodingService != null)
                    {
                        _CallOut_CodingService.Close();
                        //_CallOut_CodingService = null;
                    }

                    //Or check one of the config file variable for IP address or null
                    if (Properties.Settings.Default.CodingIP != null && Properties.Settings.Default.CodingIP != "")
                    {
                        string endpointaddress = "net.tcp://" + Properties.Settings.Default.CodingIP + ":8000/CallOut_CodingService/service";
                        EndpointAddress endpointaddr = new EndpointAddress(new Uri(endpointaddress));
                        _CallOut_CodingService = new ServiceReference1.CallOut_CodingServiceClient(new InstanceContext(this), "NetTcpBinding_CallOut_CodingService", endpointaddr);
                        _CallOut_CodingService.Open();
                    }
                }
                else //to catch the return from showdialog
                {
                    ShowMessageBox("Please check the internet connection to continue");
                }
            }
            else
            {
                ShowMessageBox("Please check the internet connection to continue");
            }
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            if (isInternetup)
            {
                if (Properties.Settings.Default.CodingIP != null && Properties.Settings.Default.CodingIP != "" && Properties.Settings.Default.CurrentID != "" && Properties.Settings.Default.CurrentID != null)
                {
                    if (_isConnected)
                    {
                        //DISCONNECTED
                        _CallOut_CodingService.ConsoleLeave(Properties.Settings.Default.CurrentID);

                        IncidentCount = 0;
                        AckCount = 0;
                        RejectedCount = 0;
                        FailedCount = 0;

                        this.txtConsoleName.Text = "";
                        this.txtIncidentAmt.Text = IncidentCount.ToString();
                        this.txtAckAmt.Text = AckCount.ToString();
                        this.txtRejectAmt.Text = RejectedCount.ToString();
                        this.txtFailedAmt.Text = FailedCount.ToString();

                        //Change title of application
                        this.Title = "Call Out Console";

                        //Toggle button display
                        _isConnected = false;

                        if (waveOut != null)
                        {
                            waveOut.Stop();
                        }
                        //Empty Display
                        EmptyDisplay();
                        IncidentMsgQueue.Clear();

                        //Update hidden label
                        lblStatus = "";
                        isfirstdisplay = true;
                        countdisplay = 1;
                        counttotal = 0;
                    }
                    else
                    {
                        //CONNECTED
                        //contact the service.
                        _CallOut_CodingService.ConsoleJoin(Properties.Settings.Default.CurrentID);

                        bool isNewStatsRecord = true;
                        foreach (StatsRecord record in StatsRecordList)
                        {
                            if (record.ConsoleName.Equals(Properties.Settings.Default.CurrentID))
                            {
                                //Continue counting from the leaveover
                                IncidentCount = record.IncidentCount;
                                AckCount = record.AckCount;
                                RejectedCount = record.RejectedCount;
                                FailedCount = record.FailedCount;
                                isNewStatsRecord = false;
                                break;
                            }
                        }

                        if (isNewStatsRecord)
                        {
                            StatsRecord newstatsrecord = new StatsRecord(Properties.Settings.Default.CurrentID, IncidentCount, AckCount, RejectedCount, FailedCount);
                            StatsRecordList.Add(newstatsrecord);
                        }
                        //else
                        //{
                        //    //Check whether count exist
                        //    foreach (StatsRecord record in StatsRecordList)
                        //    {
                        //        if (record.ConsoleName.Equals(Properties.Settings.Default.CurrentID))
                        //        {
                        //            //Continue counting from the leaveover
                        //            IncidentCount = record.IncidentCount;
                        //            AckCount = record.AckCount;
                        //            RejectedCount = record.RejectedCount;
                        //            FailedCount = record.FailedCount;
                        //            break;
                        //        }
                        //    }
                        //}
                        this.txtConsoleName.Text = Properties.Settings.Default.CurrentID;
                        this.txtIncidentAmt.Text = IncidentCount.ToString();
                        this.txtAckAmt.Text = AckCount.ToString();
                        this.txtRejectAmt.Text = RejectedCount.ToString();
                        this.txtFailedAmt.Text = FailedCount.ToString();

                        //Change title of application
                        this.Title = "Call Out Console [" + Properties.Settings.Default.CurrentID + "]";

                        //Toggle button display
                        _isConnected = true;
                    }
                }
                else 
                {
                    ShowMessageBox("Set the IP and Station ID in \"Settings\" to connect to the service");
                    this.btnConnect.IsChecked = _isConnected;
                }
            }
            else 
            {
                this.btnConnect.IsChecked = _isConnected;
                ShowMessageBox("Please check the internet connection to continue");
            }
        }

        private void btnAck_Click(object sender, RoutedEventArgs e)
        {
            if (isInternetup && !beenCutOffBefore)
            {
                if (speechSynthesizerObj != null)
                {
                    speechSynthesizerObj.Dispose();
                }
                else
                {
                    waveOut.Stop();
                }

                //increase Ack count
                foreach (StatsRecord record in StatsRecordList)
                {
                    if (record.ConsoleName.Equals(Properties.Settings.Default.CurrentID))
                    {
                        record.AckCount++;
                        break;
                    }
                }

                //Create a console log entry
                CreateConsoleLogEntry("Acknowledged");

                //Send Coding ack message back to gateway
                SendAckCodingIncidentMsg("Acknowledged");

                //disable ack and reject button
                this.btnAck.Visibility = Visibility.Collapsed;
                this.btnReject.Visibility = Visibility.Collapsed;

                EmptyDisplay();
                lblStatus = "Acknowledged";

                //Take out from msg queue and show on display
                NotifyNewMsg();
            }
            else 
            {
                ShowMessageBox("Please check the internet connection to continue");
                if (speechSynthesizerObj != null)
                {
                    speechSynthesizerObj.Dispose();
                }
                else
                {
                    waveOut.Stop();
                }

                //increase Failed Count
                foreach (StatsRecord record in StatsRecordList)
                {
                    if (record.ConsoleName.Equals(Properties.Settings.Default.CurrentID))
                    {
                        record.FailedCount++;
                        break;
                    }
                }

                CreateConsoleLogEntry("Failed");
                //Empty Display
                EmptyDisplay();
                lblStatus = "Failed";
                //Take out from msg queue and show on display
                NotifyNewMsg();
            }
        }

        private void btnReject_Click(object sender, RoutedEventArgs e)
        {
            if (isInternetup && !beenCutOffBefore)
            {
                if (speechSynthesizerObj != null)
                {
                    speechSynthesizerObj.Dispose();
                }
                else
                {
                    waveOut.Stop();
                }

                //increase Reject count
                foreach (StatsRecord record in StatsRecordList)
                {
                    if (record.ConsoleName.Equals(Properties.Settings.Default.CurrentID))
                    {
                        record.RejectedCount++;
                        break;
                    }
                }

                //Create a console log entry
                CreateConsoleLogEntry("Rejected");

                //Send Coding ack message back to gateway
                SendAckCodingIncidentMsg("Rejected");

                //Empty Display
                EmptyDisplay();

                lblStatus = "Rejected";

                //Take out from msg queue and show on display
                NotifyNewMsg();
            }
            else
            {
                ShowMessageBox("Please check the internet connection to continue");
                if (speechSynthesizerObj != null)
                {
                    speechSynthesizerObj.Dispose();
                }
                else
                {
                    waveOut.Stop();
                }

                //increase Failed Count
                foreach (StatsRecord record in StatsRecordList)
                {
                    if (record.ConsoleName.Equals(Properties.Settings.Default.CurrentID))
                    {
                        record.FailedCount++;
                        break;
                    }
                }

                CreateConsoleLogEntry("Failed");
                //Empty Display
                EmptyDisplay();
                lblStatus = "Failed";
                //Take out from msg queue and show on display
                NotifyNewMsg();
            }
        }

        private void lvUnitsColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader column = (sender as GridViewColumnHeader);
            string sortBy = column.Tag.ToString();
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(lvUnits.ItemsSource);

            if (listViewSortCol != null)
            {
                AdornerLayer.GetAdornerLayer(listViewSortCol).Remove(listViewSortAdorner);
                lvUnits.Items.SortDescriptions.Clear();
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
            lvUnits.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
        }

        private void EmptyDisplay()
        {
            this.gIncidentSummary.Visibility = Visibility.Collapsed;
            this.gPriorAlarm.Visibility = Visibility.Collapsed;
            this.gAddress.Visibility = Visibility.Collapsed;
            this.txtIncidentSummary.Text = "";
            this.txtLocationSummary.Text = "";
            this.txtPriorAlarm.Text = "";
            this.txtLocationName.Text = "";
            this.txtLocationStreet.Text = "";
            this.txtLocationUnit.Text = "";
            this.txtLocationStateCity.Text = "";
            this.txtLocationPostal.Text = "";
            this.lvUnits.Visibility = Visibility.Collapsed;

            this.btnAck.Visibility = Visibility.Collapsed;
            this.btnReject.Visibility = Visibility.Collapsed;

            //Update hidden label
            lblCodingID = "";

            //Display Stats Record 
            if (Properties.Settings.Default.CurrentID != null && Properties.Settings.Default.CurrentID != "" && _isConnected)
            {
                this.txtConsoleName.Text = Properties.Settings.Default.CurrentID;
                foreach (StatsRecord record in StatsRecordList)
                {
                    if (record.ConsoleName.Equals(Properties.Settings.Default.CurrentID))
                    {
                        //Continue counting from the leaveover
                        IncidentCount = record.IncidentCount;
                        AckCount = record.AckCount;
                        RejectedCount = record.RejectedCount;
                        FailedCount = record.FailedCount;
                        break;
                    }
                }
            }
            else 
            {
                this.txtConsoleName.Text = "";
                IncidentCount = 0;
                AckCount = 0;
                RejectedCount = 0;
                FailedCount = 0;
            }
            this.txtIncidentAmt.Text = IncidentCount.ToString();
            this.txtAckAmt.Text = AckCount.ToString();
            this.txtRejectAmt.Text = RejectedCount.ToString();
            this.txtFailedAmt.Text = FailedCount.ToString();
            this.panelStatus.Visibility = Visibility.Visible;
        }

        private void SendAckCodingIncidentMsg(string status)
        {
            
            if(isInternetup)
            {
                //cut google TTS
                waveOut.Stop();
                isMsgAlarmPlayed = false;

                CodingAckMessage codingackmsg = new CodingAckMessage();
                codingackmsg.ConsoleID = Properties.Settings.Default.CurrentID;
                codingackmsg.IncidentNo = _currIncidentNo;
                codingackmsg.CodingID = _currCodingID;
                codingackmsg.AckStatus = status;
                DateTime currentdt = DateTime.Now;
                codingackmsg.AckTimeStamp = String.Format("{0:g}", currentdt);
                List<string> dispatchUnits = new List<string>();
                foreach (UnitsStatus unitstatus in _UnitsStatusList)
                {
                    dispatchUnits.Add(unitstatus.CallSign);
                }
                codingackmsg.AckUnits = dispatchUnits.ToArray();
                _CallOut_CodingService.AckCodingIncidentMsg(codingackmsg);
            }
            else
            {
                //cut mircosoft TTS
                speechSynthesizerObj.Dispose();
                isMsgAlarmPlayed = false;
                ShowMessageBox("Please check the internet connection to continue");
            }
        }

        private void UpdateDetails(CodingIncidentMessage codingIncidentMsg)
        {

            //check whether msg is in failed msg list else display
            if (FailedCodingID.Contains(codingIncidentMsg.CodingID))
            {
                //Create a console log entry, do not need as it wont appear at all
                CreateConsoleLogEntry("Failed");

                //Empty Display
                EmptyDisplay();

                lblStatus = "Failed";

                FailedCodingID.Remove(codingIncidentMsg.CodingID);

                //Take out from msg queue and show on display
                NotifyNewMsg();
            }
            else
            {
                //increase incident count
                foreach (StatsRecord record in StatsRecordList)
                {
                    if (record.ConsoleName.Equals(Properties.Settings.Default.CurrentID))
                    {
                        record.IncidentCount++;
                        break;
                    }
                }

                //Update the Display panel details
                this.txtIncidentSummary.Text = (codingIncidentMsg.IncidentNo + ": " + codingIncidentMsg.IncidentType).ToUpper();
                this.txtLocationSummary.Text = codingIncidentMsg.IncidentType + " at " + codingIncidentMsg.IncidentLocation.Street;
                string prioralarm = "PRIORITY: " + codingIncidentMsg.IncidentPriority.ToString() +
                    "  - ALARM: " + codingIncidentMsg.IncidentAlarm.ToString() +
                    "  - DISPATCHED: " + String.Format("{0:g}", codingIncidentMsg.DispatchDateTime);
                this.txtPriorAlarm.Text = prioralarm;

                this.txtLocationName.Text = codingIncidentMsg.IncidentLocation.Name.ToUpper();
                this.txtLocationStreet.Text = codingIncidentMsg.IncidentLocation.Street;
                this.txtLocationUnit.Text = codingIncidentMsg.IncidentLocation.Unit;
                this.txtLocationStateCity.Text = "City: " + codingIncidentMsg.IncidentLocation.City +
                    " - State: " + codingIncidentMsg.IncidentLocation.State;
                this.txtLocationPostal.Text = "Singapore " + codingIncidentMsg.IncidentLocation.PostalCode;

                //Update the hidden value
                lblCodingID = codingIncidentMsg.CodingID;
                lblStatus = "";

                //Disable status record panel
                this.panelStatus.Visibility = Visibility.Collapsed;

                //the red top bar and unit listview visible
                this.gIncidentSummary.Visibility = Visibility.Visible;
                this.gPriorAlarm.Visibility = Visibility.Visible;
                this.gAddress.Visibility = Visibility.Visible;
                this.lvUnits.Visibility = Visibility.Visible;

                this.btnAck.Visibility = Visibility.Visible;
                this.btnReject.Visibility = Visibility.Visible;

                //Clear and add new unit into the list
                _UnitsStatusList.Clear();

                foreach (CodingUnits unit in codingIncidentMsg.DispatchUnits)
                {
                    if (unit.UnitCurrentStation.Equals(Properties.Settings.Default.CurrentID))
                    {
                        UnitsStatus unitstatus = new UnitsStatus();
                        unitstatus.CallSign = unit.Callsign;
                        unitstatus.UnitType = unit.UnitType;
                        _UnitsStatusList.Add(unitstatus);
                    }

                    //For test message data
                    if (unit.UnitCurrentStation.Equals("Test"))
                    {
                        UnitsStatus unitstatus = new UnitsStatus();
                        unitstatus.CallSign = unit.Callsign;
                        unitstatus.UnitType = unit.UnitType;
                        _UnitsStatusList.Add(unitstatus);
                    }
                }

                this.lvUnits.ItemsSource = null;
                this.lvUnits.ItemsSource = _UnitsStatusList;

                //Update current codingID
                _currIncidentNo = codingIncidentMsg.IncidentNo;
                _currCodingID = codingIncidentMsg.CodingID;

                //--------------------------Display Completed!!!---------------------------

                if (isInternetup)
                {
                    //Send to trigger gateway start timeout
                    _CallOut_CodingService.MsgDisplayedResponse(Properties.Settings.Default.CurrentID, codingIncidentMsg);
                }

                //Text to speech
                string IncidentNoSpeech = "Incident Number, " + codingIncidentMsg.IncidentNo + ". ";
                //string IncidentnoSpeech = "Incident Number : " + codingIncidentMsg.IncidentNo + ", Incident Type : "+ codingIncidentMsg.IncidentType;
                string PriorAlarmSpeech = "PRIORITY, " + codingIncidentMsg.IncidentPriority.ToString() +
                    "  . ALARM, " + codingIncidentMsg.IncidentAlarm.ToString() +
                    "  . DISPATCHED, " + String.Format("{0:g}", codingIncidentMsg.DispatchDateTime);
                speechsentence = IncidentNoSpeech + this.txtLocationSummary.Text + ". " + PriorAlarmSpeech;
                string strURL = string.Format(URL, "en", speechsentence.ToLower().Replace(" ", "%20"));

                WaveFileReader wavreader = new WaveFileReader(Properties.Resources.incomingmsg);
                waveOut = new WaveOut();
                waveOut.Init(wavreader);
                waveOut.Play();
                isMsgAlarmPlayed = true;
                System.Timers.Timer MsgAlarmTimer = new System.Timers.Timer();
                MsgAlarmTimer.Interval = wavreader.Length/100; //30 sec
                MsgAlarmTimer.Elapsed += delegate { MsgAlarmCompleted(strURL, speechsentence); };
                MsgAlarmTimer.AutoReset = false;
                MsgAlarmTimer.Start();


                //Start timer for 10 sec to auto reject, tag with coding ID more unqiue
                System.Timers.Timer AutoRejectTimer = new System.Timers.Timer();
                AutoRejectTimer.Interval = 30000; //30 sec
                AutoRejectTimer.Elapsed += delegate { Timeout(codingIncidentMsg.CodingID); };
                AutoRejectTimer.AutoReset = false;
                AutoRejectTimer.Start();
            }
        }

        private void MsgAlarmCompleted(string strURL, string speechsentence)
        {
            if (isMsgAlarmPlayed)
            {
                waveOut.Stop();
                isMsgAlarmPlayed = false;

                if (isInternetup)
                {
                    GenerateSpeechFromText(strURL);
                }
                else
                {
                    speechSynthesizerObj = new SpeechSynthesizer();
                    speechSynthesizerObj.SpeakAsync(speechsentence);
                    speechsentence = "";
                }
            }
        }

        #region Methods for Text to Speech

        private void GenerateSpeechFromText(string sentence)
        {
            WebClient serviceRequest = new WebClient();
            serviceRequest.DownloadDataCompleted += serviceRequest_DownloadDataCompleted;

            try
            {
                serviceRequest.DownloadDataAsync(new Uri(sentence));
            }
            catch (Exception)
            {
                speechSynthesizerObj = new SpeechSynthesizer();
                speechSynthesizerObj.SpeakAsync(speechsentence);
                speechsentence = "";
                //MessageBox.Show("An error occured while requesting Google REST API !");
            }
        }

        void serviceRequest_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            if (e.Error == null && e.Result != null)
            {
                // play MP3 using nAudio lib
                PlayMP3(e.Result);
            }
            else
            {
                speechSynthesizerObj = new SpeechSynthesizer();
                speechSynthesizerObj.SpeakAsync(speechsentence);
                speechsentence = "";
                //MessageBox.Show("An error occured while requesting Google REST API !");
            }
        }

        private void PlayMP3(byte[] soundDataArray)
        {
            Stream stream = new MemoryStream(soundDataArray);

            if (stream != null)
            {
                Mp3FileReader mp3reader = new Mp3FileReader(stream);
                waveOut.Init(mp3reader);
                waveOut.Play();
            }
        }

        #endregion

        public void Timeout(string codingID)
        {
            SendOrPostCallback callback =
                    delegate(object state)
                    {
                        
                            //Already ack or reject 10 sec do nth, timeout tag withincident no so wont affect other
                            if (lblStatus.Equals("") && lblCodingID.Equals(codingID))
                            {
                                if (isInternetup)
                                {
                                    //increase Reject count
                                    foreach (StatsRecord record in StatsRecordList)
                                    {
                                        if (record.ConsoleName.Equals(Properties.Settings.Default.CurrentID))
                                        {
                                            record.RejectedCount++;
                                            break;
                                        }
                                    }

                                    //Create a console log entry
                                    CreateConsoleLogEntry("Rejected");

                                    //Send Coding ack message back to gateway
                                    SendAckCodingIncidentMsg("Rejected");

                                    lblStatus = "Rejected";
                                }
                                else
                                {
                                    //increase Failed count
                                    foreach (StatsRecord record in StatsRecordList)
                                    {
                                        if (record.ConsoleName.Equals(Properties.Settings.Default.CurrentID))
                                        {
                                            record.FailedCount++;
                                            break;
                                        }
                                    }

                                    //ShowMessageBox("Please check the internet connection to continue");
                                    CreateConsoleLogEntry("Failed");
                                    lblStatus = "Failed";
                                }

                                lblCodingID = "";

                                //Take out from msg queue and show on display
                                NotifyNewMsg();
                            }
                        
                    };

            _uiSyncContext.Post(callback, "Message Timeout");

        }

        private void CreateConsoleLogEntry(string status)
        {
            string consolelog = Properties.Settings.Default.CurrentID;
            consolelog += "," + _currCodingID;
            DateTime currentdt = DateTime.Now;
            consolelog += "," + String.Format("{0:g}", currentdt);
            consolelog += ",Gateway"; //Static?
            consolelog += "," + status;
            Properties.Settings.Default.ConsoleLogList.Add(consolelog);
            Properties.Settings.Default.Save();
        }

        private void NotifyNewMsg()
        {
            //Only attempt to take out from queue if there is msg inside the queue
            if (IncidentMsgQueue.Count != 0)
            {
                if (!isfirstdisplay)
                {
                    countdisplay++;
                }

                if (isInternetup && !beenCutOffBefore)
                {
                    this.Title = "Call Out Console [" + Properties.Settings.Default.CurrentID + "] (" + countdisplay.ToString() + " of " + counttotal.ToString() + ")";
                }
                else 
                {
                    this.Title = "Call Out Console (" + countdisplay.ToString() + " of " + counttotal.ToString() + ")";
                }
                
                this.UpdateDetails(IncidentMsgQueue.Dequeue());
                isfirstdisplay = false;
            }
            else
            {
                beenCutOffBefore = false;
                countdisplay = 1;
                counttotal = 0;
                if (isInternetup)
                {
                    this.Title = "Call Out Console [" + Properties.Settings.Default.CurrentID + "]";
                }
                else 
                {
                    this.Title = "Call Out Console";
                }
                isfirstdisplay = true;
                EmptyDisplay();
            }

        }

        #region CallOut_CodingServiceCallback Methods

        public void ConsoleDisplayMsg(CodingIncidentMessage codingIncidentMsg)
        {
            SendOrPostCallback callback =
            delegate(object state)
            {
                if (isInternetup)
                {
                    //Update to the queue
                    IncidentMsgQueue.Enqueue(codingIncidentMsg);
                    counttotal++; //increase number of msg on queue
                    this.Title = "Call Out Console [" + Properties.Settings.Default.CurrentID + "] (" + countdisplay.ToString() + " of " + counttotal.ToString() + ")";
                    //If currently does not servicing any message
                    if (lblCodingID.Equals(""))
                    {
                        NotifyNewMsg();
                    }
                }
                else
                {
                    ShowMessageBox("Please check the internet connection to continue");
                }
            };

            _uiSyncContext.Post(callback, "updatedetails");
        }

        public void ConsoleRcvConnStatus()
        {
            SendOrPostCallback callback =
                delegate(object state)
                {
                    if (isInternetup)
                    {
                        _CallOut_CodingService.ReplyConnStatus(Properties.Settings.Default.CurrentID);
                    }
                    else
                    {
                        ShowMessageBox("Please check the internet connection to continue");
                    }
                    
                };

            _uiSyncContext.Post(callback, "rcv conn status request");
        }

        public void UpdateRemoveMsgList(string CodingID)
        {
            SendOrPostCallback callback =
                delegate(object state)
                {
                    if (isInternetup)
                    {
                        FailedCodingID.Add(CodingID);

                        //If current display is msg of codingID, throw away with failed
                        if (lblCodingID.Equals(CodingID))
                        {
                            //Create a console log entry
                            CreateConsoleLogEntry("Failed");

                            //Empty Display
                            EmptyDisplay();

                            lblStatus = "Failed";

                            //Take out from msg queue and show on display
                            NotifyNewMsg();
                        }
                    }
                    else
                    {
                        ShowMessageBox("Please check the internet connection to continue");
                    }
                    
                };

            _uiSyncContext.Post(callback, "update list of msg that will be remove");
        }

        #region Methods not for Console

        public void EditConnStatus(string Name, string Status)
        { }
        public void RcvCodingAckMsg(CodingAckMessage codingAckMsg)
        { }
        public void NotifyConsoleNotConnected(string userName, CodingIncidentMessage codingIncidentMsg)
        { }
        public void GatewayRcvConnStatus(string station)
        { }
        public void StartTargetTimeoutTimer(string console, CodingIncidentMessage codingIncidentMsg) 
        { }

        #endregion

        #endregion
    }
}
