using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Threading;
//using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
//using System.Windows.Input;
//using System.Windows.Media;
//using System.Windows.Media.Imaging;
//using System.Windows.Navigation;
//using System.Windows.Shapes;
using System.ServiceModel;
using System.Diagnostics; //for debug
using System.ComponentModel; //for bindinglist, inotify
using System.Timers; //Timer
using NAudio.Wave;
using System.Net;
using System.IO;

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
        private string _currCodingID = "";

        //For sort at column header
        private GridViewColumnHeader listViewSortCol = null;
        private SortAdorner listViewSortAdorner = null;

        //Default string to find the translate
        private const string URL = "http://translate.google.com/translate_tts?tl={0}&q={1}";
        
        public MainWindow()
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

            //DataGrid Bind
            _UnitsStatusList.Clear();
            this.lvUnits.ItemsSource = _UnitsStatusList;

            //hide accept and reject button
            this.btnAck.Visibility = Visibility.Collapsed;
            this.btnReject.Visibility = Visibility.Collapsed;

        }

        private void MyWindow_Closing(object sender, CancelEventArgs e)
        {
            //Terminate the connection to the service.
            if (_isConnected)
            {
                _CallOut_CodingService.Close();
            }

        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow winsetting = new SettingsWindow();
            winsetting.ShowDialog();
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            if (_isConnected)
            {
                //DISCONNECTED
                // Let the service know that this user is leaving
                _CallOut_CodingService.ConsoleLeave(Properties.Settings.Default.CurrentID);

                //Change title of application
                this.Title = "Call Out Console";

                //Toggle button display
                _isConnected = false;

                //Empty Display
                EmptyDisplay();

                //Update hidden label
                this.lblStatus.Text = "";

            }
            else
            {
                //CONNECTED
                //contact the service.
                _CallOut_CodingService.ConsoleJoin(Properties.Settings.Default.CurrentID);

                //Change title of application
                this.Title = "Call Out Console [" + Properties.Settings.Default.CurrentID + "]";

                //Toggle button display
                _isConnected = true;
            }
        }

        private void btnAck_Click(object sender, RoutedEventArgs e)
        {
            //Create a console log entry
            CreateConsoleLogEntry("Acknowledged");

            //Send Coding ack message back to gateway
            SendAckCodingIncidentMsg("Acknowledged");

            //disable ack and reject button
            this.btnAck.Visibility = Visibility.Collapsed;
            this.btnReject.Visibility = Visibility.Collapsed;

            this.lblCodingID.Text = "";
            this.lblStatus.Text = "Acknowledged";

            //Take out from msg queue and show on display
            NotifyNewMsg();
        }

        private void btnReject_Click(object sender, RoutedEventArgs e)
        {
            //Create a console log entry
            CreateConsoleLogEntry("Rejected");

            //Send Coding ack message back to gateway
            SendAckCodingIncidentMsg("Rejected");

            //disable ack and reject button
            this.btnAck.Visibility = Visibility.Collapsed;
            this.btnReject.Visibility = Visibility.Collapsed;

            //Empty Display
            EmptyDisplay();

            this.lblStatus.Text = "Rejected";

            //Take out from msg queue and show on display
            NotifyNewMsg();
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
            this.txtIncidentSummary.Text = "";
            this.txtLocationSummary.Text = "";
            this.txtPriorAlarm.Text = "";
            this.txtLocationName.Text = "";
            this.txtLocationStreet.Text = "";
            this.txtLocationUnit.Text = "";
            this.txtLocationStateCity.Text = "";
            this.txtLocationPostal.Text = "";
            this.lvUnits.Visibility = Visibility.Collapsed;

            //Update hidden label
            this.lblCodingID.Text = "";
        }

        private void SendAckCodingIncidentMsg(string status)
        {
            CodingAckMessage codingackmsg = new CodingAckMessage();
            codingackmsg.ConsoleID = Properties.Settings.Default.CurrentID;
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

        private void UpdateDetails(CodingIncidentMessage codingIncidentMsg)
        {
            //check whether msg is in failed msg list else display
            //if (FailedCodingID.Contains(codingIncidentMsg.CodingID))
            //{
            //    //Create a console log entry, do not need as it wont appear at all
            //    //CreateConsoleLogEntry("Failed");

            //    //disable ack and reject button
            //    this.btnAck.Visibility = Visibility.Collapsed;
            //    this.btnReject.Visibility = Visibility.Collapsed;

            //    //Empty Display
            //    EmptyDisplay();

            //    this.lblStatus.Text = "Failed";

            //    FailedCodingID.Remove(codingIncidentMsg.CodingID);

            //    //Take out from msg queue and show on display
            //    NotifyNewMsg();
            //}
            //else
            //{
                //Update the Display panel details
                this.txtIncidentSummary.Text = codingIncidentMsg.IncidentNo + ": " + codingIncidentMsg.IncidentType;
                this.txtLocationSummary.Text = codingIncidentMsg.IncidentType + " at " + codingIncidentMsg.IncidentLocation.Street;
                string prioralarm = "PRIORITY: " + codingIncidentMsg.IncidentPriority.ToString() +
                    "  - ALARM: " + codingIncidentMsg.IncidentAlarm.ToString() +
                    "  - DISPATCHED: " + String.Format("{0:g}", codingIncidentMsg.DispatchDateTime);
                this.txtPriorAlarm.Text = prioralarm;

                this.txtLocationName.Text = codingIncidentMsg.IncidentLocation.Name;
                this.txtLocationStreet.Text = codingIncidentMsg.IncidentLocation.Street;
                this.txtLocationUnit.Text = codingIncidentMsg.IncidentLocation.Unit;
                this.txtLocationStateCity.Text = "City: " + codingIncidentMsg.IncidentLocation.City +
                    " - State: " + codingIncidentMsg.IncidentLocation.State;
                this.txtLocationPostal.Text = "Singapore " + codingIncidentMsg.IncidentLocation.PostalCode;

                //Update the hidden value
                this.lblCodingID.Text = codingIncidentMsg.CodingID;
                this.lblStatus.Text = "";

                //the red top bar and unit listview visible
                this.gIncidentSummary.Visibility = Visibility.Visible;
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
                    if (unit.UnitCurrentStation.Equals(""))
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
                _currCodingID = codingIncidentMsg.CodingID;

                //--------------------------Display Completed!!!---------------------------

                //Send to trigger gateway start timeout
                _CallOut_CodingService.MsgDisplayedResponse(Properties.Settings.Default.CurrentID, codingIncidentMsg);

                //Text to speech
                string IncidentNoSpeech = "Incident Number, " + codingIncidentMsg.IncidentNo + ". ";
                //string IncidentnoSpeech = "Incident Number : " + codingIncidentMsg.IncidentNo + ", Incident Type : "+ codingIncidentMsg.IncidentType;
                string PriorAlarmSpeech = "PRIORITY, " + codingIncidentMsg.IncidentPriority.ToString() +
                    "  . ALARM, " + codingIncidentMsg.IncidentAlarm.ToString() +
                    "  . DISPATCHED, " + String.Format("{0:g}", codingIncidentMsg.DispatchDateTime);
                string speechsentence = IncidentNoSpeech + this.txtLocationSummary.Text + ". " + PriorAlarmSpeech;
                string strURL = string.Format(URL, "en", speechsentence.ToLower().Replace(" ", "%20"));
                GenerateSpeechFromText(strURL);


                //Start timer for 10 sec to auto reject, tag with coding ID more unqiue
                System.Timers.Timer AutoRejectTimer = new System.Timers.Timer();
                AutoRejectTimer.Interval = 10000; //10 sec
                AutoRejectTimer.Elapsed += delegate { Timeout(codingIncidentMsg.CodingID); };
                AutoRejectTimer.AutoReset = false;
                AutoRejectTimer.Start();
            //}
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
                MessageBox.Show("An error occured while requesting Google REST API !");
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
                MessageBox.Show("An error occured while requesting Google REST API !");
            }
        }

        private void PlayMP3(byte[] soundDataArray)
        {
            Stream stream = new MemoryStream(soundDataArray);

            if (stream != null)
            {
                Mp3FileReader reader = new Mp3FileReader(stream);
                var waveOut = new WaveOut();
                waveOut.Init(reader);
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
                        if (this.lblStatus.Text.Equals("") && this.lblCodingID.Text.Equals(codingID))
                        {
                            //Create a console log entry
                            CreateConsoleLogEntry("Rejected");

                            //Send Coding ack message back to gateway
                            SendAckCodingIncidentMsg("Rejected");

                            //disable ack and reject button
                            this.btnAck.Visibility = Visibility.Collapsed;
                            this.btnReject.Visibility = Visibility.Collapsed;

                            this.lblCodingID.Text = "";
                            this.lblStatus.Text = "Rejected";

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
            //_ConsoleLogList.Add(consolelog);
            Properties.Settings.Default.ConsoleLogList.Add(consolelog);
            Properties.Settings.Default.Save();
        }

        private void NotifyNewMsg()
        {
            //Only attempt to take out from queue if there is msg inside the queue
            if (IncidentMsgQueue.Count != 0)
            {
                this.UpdateDetails(IncidentMsgQueue.Dequeue());
            }
        }


        #region CallOut_CodingServiceCallback Methods

        public void ConsoleDisplayMsg(CodingIncidentMessage codingIncidentMsg)
        {
            SendOrPostCallback callback =
            delegate(object state)
            {
                //Update to the queue
                IncidentMsgQueue.Enqueue(codingIncidentMsg);
                //If currently does not servicing any message
                if (this.lblCodingID.Text.Equals(""))
                {
                    NotifyNewMsg();
                }
            };

            _uiSyncContext.Post(callback, "updatedetails");
        }

        public void ConsoleRcvConnStatus()
        {
            SendOrPostCallback callback =
                delegate(object state)
                {
                    _CallOut_CodingService.ReplyConnStatus(Properties.Settings.Default.CurrentID);
                };

            _uiSyncContext.Post(callback, "rcv conn status request");
        }

        public void UpdateRemoveMsgList(string CodingID)
        {
            SendOrPostCallback callback =
                delegate(object state)
                {
                    FailedCodingID.Add(CodingID);

                    //If current display is msg of codingID, throw away with failed
                    if (this.lblCodingID.Text.Equals(CodingID))
                    {
                        //Create a console log entry
                        CreateConsoleLogEntry("Failed");

                        //disable ack and reject button
                        this.btnAck.Visibility = Visibility.Collapsed;
                        this.btnReject.Visibility = Visibility.Collapsed;

                        //Empty Display
                        EmptyDisplay();

                        this.lblStatus.Text = "Failed";

                        //Take out from msg queue and show on display
                        NotifyNewMsg();
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
