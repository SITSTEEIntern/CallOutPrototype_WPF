using System;
using System.Collections.Generic;
using System.ComponentModel; //for bindinglist
using System.Drawing;
using System.Threading;
using System.ServiceModel;
using System.Windows.Forms;
using System.Diagnostics; //for debug
using System.Timers;
using System.IO;

//From CAD ppl
using gCAD.Shared.IntegrationContract;
using gCAD.Integration.AppLib;
using System.Configuration;
using CalloutServices;

//using System.Linq;
//using System.Text;
//using System.Data;

// Location of the proxy.
using CallOut_Gateway.ServiceReference1; //Coding
using CallOut_Gateway.ServiceReference2; //CAD

using CallOut_Gateway.Class;

namespace CallOut_Gateway
{
    // Specify for the callback to NOT use the current synchronization context
    [CallbackBehavior(
        ConcurrencyMode = ConcurrencyMode.Single,
        UseSynchronizationContext = false)]
    public partial class Form1 : Form, ServiceReference1.CallOut_CodingServiceCallback, ServiceReference2.CallOut_CADServiceCallback
    {
        private SynchronizationContext _uiSyncContext = null;

        private ConfigSettings configsetting = new ConfigSettings(); //to overwrite the endpoint address in appconfig on runtime
        private ServiceReference1.CallOut_CodingServiceClient _CallOut_CodingService = null;
        private ServiceReference2.CallOut_CADServiceClient _CallOut_CADService = null;

        //Flags
        private bool _isCADConnected = false;
        private bool _isConsoleConnected = false;

        private bool _isSimulateCADSvcIPSet = false;
        private bool _isCodingSvcIPSet = false;
        private bool _ActivateHealthCheck = false;

        //For binding of list to datagirdview
        //Bindinglist allow user to add row, however will left a empty last row.
        private BindingList<CodingStatus> _CodingStatusList = new BindingList<CodingStatus>();
        private BindingList<MessageStatus> _MessageStatusList = new BindingList<MessageStatus>();
        private List<StationStatus> _StationStatusList = new List<StationStatus>();

        //List of informaton at is hold and track at gateway, e.g station status and dispatchunit info for adhoc request
        //pending and completed list?
        private List<GatewayTracker> _GatewayTrackerList = new List<GatewayTracker>();

        //List of station that will be remove from service
        private List<string> _ToBeRemove;

        //-----------------From Call Out Simulator----------------------
        private ClientSession session = null; //remove readonly as I cannot just set at constructor
        private IncidentIntegrationClient incidentSession = null;
        private bool _isRealCADSvcIPSet = false;
        private bool _isServiceStart = false;
        public static ServiceHost host = null;

        private static volatile Form1 _instance;
        private readonly static object _synRoot = new object();

        public static Form1 Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_synRoot)
                    {
                        if (_instance == null)
                            _instance = new Form1();
                    }
                }
                return _instance;
            }
        }

        private Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Capture the UI synchronization context
            _uiSyncContext = SynchronizationContext.Current;

            // Initial eventhandlers
            this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);

            //Fill in the IP address from IP config file
            string[] lineOfContents = File.ReadAllLines(@"..\..\..\..\IPConfigFile.txt");
            this.txtRealCADSvcIP.Text = lineOfContents[1].Trim();
            this.txtSimulateCADSvcIP.Text = lineOfContents[2].Trim();
            this.txtCodingSvcIP.Text = lineOfContents[3].Trim();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //Terminate the connection to the service.
            if (_isCodingSvcIPSet)
            {
                _CallOut_CodingService.Close();
            }
            if (_isSimulateCADSvcIPSet)
            {
                _CallOut_CADService.Close();
            }
        }

        private static void Log(string logMessage) 
        {
            DateTime currentdt = DateTime.Now;
            string date = currentdt.Year + "-" + currentdt.Month + "-" + currentdt.Day;
            TextWriter tw = new StreamWriter(@"..\..\..\..\Log\"+date+".txt", true);
            tw.WriteLine("[" + currentdt.ToString() + "] [Gateway] \"" + logMessage + "\"");
            tw.Close();
        }

        #region From Call Out Simulator

        private void btnSetRealCADSvcIP_Click(object sender, EventArgs e)
        {
            if (_isRealCADSvcIPSet)
            {
                //Unset

                //Need to cut off the client proxy with CAD
                //Need confirmation!!!
                session = null;
                incidentSession = null;

                this.txtRealCADSvcIP.Enabled = true;
                this.btnSetRealCADSvcIP.Text = "Set";
                _isRealCADSvcIPSet = false;
            }
            else
            {
                //Set
                try
                {
                    Log("Set session and incident session");
                    //This part need to edit (How does they link up the Endpoint, in order for me to dynamic change IP)
                    //sessionEP and incidentSessionEP just return a string that corresponding to key in appconfig
                    //How does it access to the appconfig endpoint setting
                    string sessionEPaddress = "net.tcp://" + this.txtRealCADSvcIP.Text.Trim() + ":2044/IncidentIntegrationService/";
                    var sessionEP = ConfigurationManager.AppSettings.Get("SessionEndPoint");
                    configsetting.SaveEndpointAddress(sessionEPaddress, sessionEP); //overwrite endpoint address in appconfig
                    
                    string incidentSessionEPaddress = "net.tcp://" + this.txtRealCADSvcIP.Text.Trim() + ":2045/IntegrationSessionService/";
                    var incidentSessionEP = ConfigurationManager.AppSettings.Get("IncidentIntegrationServiceEndPoint");
                    configsetting.SaveEndpointAddress(incidentSessionEPaddress, incidentSessionEP); //overwrite endpoint address in appconfig

                    ConfigurationManager.RefreshSection("system.serviceModel/client");

                    session = new ClientSession(sessionEP);
                    incidentSession = new IncidentIntegrationClient(incidentSessionEP);

                    this.txtRealCADSvcIP.Enabled = false;
                    //Change button Text
                    this.btnSetRealCADSvcIP.Text = "Unset";
                    _isRealCADSvcIPSet = true;
                }
                catch(Exception exception)
                {
                    Log(exception.ToString());
                    MessageBox.Show("Invalid IP address...");
                }

            }
        }

        private void btnStartService_Click(object sender, EventArgs e)
        {
            if (_isServiceStart)
            {
                //Service Stopped

                host.Close();

                this.btnStartService.Text = "Start Service";
                _isServiceStart = false;
            }
            else
            {
                Log("Start service to receive dispatched incident");
                //Service started (so gateway act as a server to receive incident from CAD which act as client)
                host = new ServiceHost(typeof(DispatchService));
                host.Open();

                //Change button Text
                this.btnStartService.Text = "Stop Service";
                _isServiceStart = true;

            }
        }

        private void EmptyAllDataGridView()
        {
            this.dgvCoding.DataSource = null;
            this.dgvCoding.Columns.Clear();

            this.dgvMessage.DataSource = null;
            this.dgvMessage.Columns.Clear();

            this.dgvStation.CellPainting -= dgvStation_CellPainting;
            this.dgvStation.CellValueChanged -= dgvStation_CellValueChanged;
            this.dgvStation.DataSource = null;
            this.dgvStation.Columns.Clear();
        }

        private bool CreateClientSession()
        {
            Log("Create Client Session");
            if (session.IsInitiated == false)
            {
                var username = ConfigurationManager.AppSettings.Get("CadUsername");
                var password = ConfigurationManager.AppSettings.Get("CadPassword");

                return session.Initiate(username, password, AccountType.Proprietary);
            }
            else return true;
        }

        //Passing back to CAD
        private void CallAPI(string IncidentNumber, string Comment)
        {
            try
            {
                Log("Sending message back to CAD");
                if (CreateClientSession())
                {
                    incidentSession.AddIncidentComment(IncidentNumber, "", Comment);
                    Log("Incident Number: " + IncidentNumber + " Successfully updated the Comment to: " + Comment);
                    //MessageBox.Show("Incident Number: " + IncidentNumber + " Successfully updated the Comment to: " + Comment);

                }
            }
            catch (Exception ex)
            {
                Log("Update Comment Failed.");
                Log(ex.ToString());
                //MessageBox.Show("Update Comment Failed.");
            }
        }

        //------------------------------- for trying connection purpose---------------------------------------------
        public void RcvDispatchedIncidentMsg(gCAD.Shared.IntegrationContract.DispatchedIncident CADincidentMsg)
        {
            SendOrPostCallback callback =
                delegate (object state)
                {
                    List<string> tmpstationList = new List<string>();
                    List<Tracking> trackingList = new List<Tracking>();
                    //Only take out the stations on the Current Station
                    foreach (gCAD.Shared.IntegrationContract.DispatchedUnit uniqueunit in CADincidentMsg.ListOfUnits)
                    {
                        //Avoid duplicate station name in the list
                        if (!tmpstationList.Contains(uniqueunit.CurrentStation))
                        {
                            tmpstationList.Add(uniqueunit.CurrentStation);
                            Tracking newstation = new Tracking();
                            newstation.Station = uniqueunit.CurrentStation;
                            newstation.Status = "Pending";

                            List<string> unitcallsign = new List<string>();

                            //To give relevant station units callsign
                            foreach (gCAD.Shared.IntegrationContract.DispatchedUnit unit in CADincidentMsg.ListOfUnits)
                            {
                                if (unit.CurrentStation.Equals(uniqueunit.CurrentStation))
                                {
                                    unitcallsign.Add(unit.CallSign);
                                }
                            }

                            newstation.Unit = unitcallsign.ToArray();
                            trackingList.Add(newstation); //Add into tracking list
                        }
                    }

                    string[] addressList = tmpstationList.ToArray();

                    //Convert Incident to Coding Message and send to respective console
                    CodingIncidentMessage codingincidentmsg = ConvertDispatchedIncidentToCoding(CADincidentMsg);
                    ConvertCodingtoTracker(addressList, codingincidentmsg); //Add to gateway tracker
                    Log("Forward message from Real CAD to respective console");
                    _CallOut_CodingService.TargetMsg(addressList, codingincidentmsg);

                    //Send ack back to CAD
                    foreach (Tracking station in trackingList)
                    {
                        string Comment = station.Station + " " + station.Status;
                        CallAPI(CADincidentMsg.IncidentNumber, Comment);
                    }

                    //Set Coding Entry
                    CreateCodingEntry(codingincidentmsg, tmpstationList.Count.ToString());
                    //Set Message Entry
                    CreateMessageEntry(codingincidentmsg, tmpstationList.Count.ToString());

                };

            _uiSyncContext.Post(callback, "Rcv Incident Message");
        }

        public CodingIncidentMessage ConvertDispatchedIncidentToCoding(gCAD.Shared.IntegrationContract.DispatchedIncident CADincidentMsg)
        {
            string codingID = _CallOut_CodingService.GetCodingID();

            CodingLocation incidentLocation = new CodingLocation();
            incidentLocation.Name = "N.A. Name";
            //incidentLocation.Name = CADincidentMsg.IncidentLocation.Name;
            incidentLocation.Street = CADincidentMsg.IncidentLocation.Street;
            incidentLocation.Unit = CADincidentMsg.IncidentLocation.Unit;
            incidentLocation.State = CADincidentMsg.IncidentLocation.State;
            incidentLocation.City = CADincidentMsg.IncidentLocation.City;
            incidentLocation.Country = "N.A. Country";
            //incidentLocation.Country = CADincidentMsg.IncidentLocation.Country;
            incidentLocation.PostalCode = CADincidentMsg.IncidentLocation.PostalCode;

            List<CodingUnits> tmpcodingunitList = new List<CodingUnits>();
            foreach (gCAD.Shared.IntegrationContract.DispatchedUnit unit in CADincidentMsg.ListOfUnits)
            {
                CodingUnits newUnit = new CodingUnits();
                newUnit.ID = unit.ID;
                newUnit.Callsign = unit.CallSign;
                newUnit.UnitType = "N.A. Unit Type";
                //newUnit.UnitType = unit.UnitType;
                newUnit.FromStatus = unit.Status;
                newUnit.UnitLocation = unit.Location;
                newUnit.UnitHomeStation = unit.HomeStation;
                newUnit.UnitCurrentStation = unit.CurrentStation;

                tmpcodingunitList.Add(newUnit);
            }

            CodingUnits[] dispatchUnits = tmpcodingunitList.ToArray();

            CodingIncidentMessage codingIncidentMsg = new CodingIncidentMessage();
            codingIncidentMsg.CodingID = codingID;
            codingIncidentMsg.IncidentNo = CADincidentMsg.IncidentNumber;
            codingIncidentMsg.IncidentTitle = "N.A. Incident Title";
            //codingIncidentMsg.IncidentTitle = CADincidentMsg.IncidentTitle;
            codingIncidentMsg.IncidentLocation = incidentLocation;
            codingIncidentMsg.IncidentType = CADincidentMsg.IncidentType;
            codingIncidentMsg.IncidentAlarm = CADincidentMsg.AlarmLevel;
            codingIncidentMsg.IncidentPriority = CADincidentMsg.Priority;
            codingIncidentMsg.DispatchDateTime = CADincidentMsg.DateTime;
            codingIncidentMsg.DispatchUnits = dispatchUnits;

            return codingIncidentMsg;
        }

        //------------------------------- for trying connection purpose---------------------------------------------

        #endregion

        #region Simulate CAD and Console

        private void btnSetSimulateCADSvcIP_Click(object sender, EventArgs e)
        {
            if (_isSimulateCADSvcIPSet)
            {
                if (!_isCADConnected)
                {
                    //Un set

                    //Cut off the proxy / channel
                    _CallOut_CADService.Close();

                    this.txtSimulateCADSvcIP.Enabled = true;
                    this.btnSetSimulateCADSvcIP.Text = "Set";
                    _isSimulateCADSvcIPSet = false;
                }
                else
                {
                    MessageBox.Show("Leave CAD before unset the IP address...");
                }
            }
            else
            {
                //Set
                try
                {
                    Log("Open Simulate CAD service proxy");
                    string endpointaddress = "net.tcp://" + this.txtSimulateCADSvcIP.Text.Trim() + ":8002/CallOut_CADService/service";
                    _CallOut_CADService = new ServiceReference2.CallOut_CADServiceClient(new InstanceContext(this), "NetTcpBinding_CallOut_CADService", endpointaddress);
                    _CallOut_CADService.Open();

                    this.txtSimulateCADSvcIP.Enabled = false;
                    //Change button Text
                    this.btnSetSimulateCADSvcIP.Text = "Unset";
                    _isSimulateCADSvcIPSet = true;
                }
                catch (Exception exception)
                {
                    Log(exception.ToString());
                    MessageBox.Show("Invalid IP address...");
                }
            }
        }

        private void btnJoinCAD_Click(object sender, EventArgs e)
        {
            if (_isSimulateCADSvcIPSet)
            {
                if (_isCADConnected)
                {
                    Log("Gateway Leave CAD Service");
                    // Let the service know that this user is leaving
                    _CallOut_CADService.GatewayLeave();

                    //Toggle button display
                    _isCADConnected = false;
                    this.btnJoinCAD.Text = "Join CAD";
                }
                else
                {
                    try
                    {
                        Log("Gateway Join CAD Service");
                        //contact the service.
                        _CallOut_CADService.GatewayJoin();

                        //Toggle button display
                        _isCADConnected = true;
                        this.btnJoinCAD.Text = "Leave CAD";
                    }
                    catch (Exception exception)
                    {
                        Log(exception.ToString());
                        MessageBox.Show("IP address had not been set...");
                    }

                }
            }
            else 
            {
                MessageBox.Show("IP address had not been set...");
            }

        }

        private void btnSetCodingSvcIP_Click(object sender, EventArgs e)
        {
            if (_isCodingSvcIPSet)
            {
                if (!_isConsoleConnected)
                {
                    //Un set

                    //Clear Datagrid
                    EmptyAllDataGridView();
                    _ActivateHealthCheck = false;

                    //Cut off the proxy / channel
                    _CallOut_CodingService.Close();

                    this.txtCodingSvcIP.Enabled = true;
                    this.btnSetCodingSvcIP.Text = "Set";
                    _isCodingSvcIPSet = false;
                }
                else
                {
                    MessageBox.Show("Leave Console before unset the IP address...");
                }
            }
            else
            {
                //Set
                try
                {
                    Log("Open Coding Service proxy");
                    string endpointaddress = "net.tcp://" + this.txtCodingSvcIP.Text.Trim() + ":8000/CallOut_CodingService/service";
                    _CallOut_CodingService = new ServiceReference1.CallOut_CodingServiceClient(new InstanceContext(this), "NetTcpBinding_CallOut_CodingService", endpointaddress); //using new endpointaddress
                    _CallOut_CodingService.Open();

                    //Set Up DataGridView
                    InitCodingDataGrid();
                    InitMessageDataGrid();
                    InitStationDataGrid();

                    //Init Health Checker aka check for ocnsole connectivity
                    _ActivateHealthCheck = true;
                    HealthCheck();

                    this.txtCodingSvcIP.Enabled = false;
                    //Change button Text
                    this.btnSetCodingSvcIP.Text = "Unset";
                    _isCodingSvcIPSet = true;
                }
                catch (Exception exception)
                {
                    Log(exception.ToString());
                    MessageBox.Show("Invalid IP address...");
                }

            }
        }

        private void btnJoinConsole_Click(object sender, EventArgs e)
        {
            if (_isCodingSvcIPSet)
            {
                if (_isConsoleConnected)
                {
                    Log("Gateway Leave Coding Service");
                    // Let the service know that this user is leaving
                    _CallOut_CodingService.GatewayLeave();

                    //Toggle button display
                    _isConsoleConnected = false;
                    this.btnJoinConsole.Text = "Join Console";
                }
                else
                {
                    try
                    {
                        Log("Gateway join Coding Service");
                        //contact the service.
                        _CallOut_CodingService.GatewayJoin();

                        //Toggle button display
                        _isConsoleConnected = true;
                        this.btnJoinConsole.Text = "Leave Console";
                    }
                    catch (Exception exception)
                    {
                        Log(exception.ToString());
                        MessageBox.Show("IP address had not been set...");
                    }
                }
            }
            else 
            {
                MessageBox.Show("IP address had not been set...");
            }
        }

        #endregion

        #region Health Check Operations

        private void InitToBeRemove()
        {
            _ToBeRemove = new List<string>();
            foreach (string station in _CallOut_CodingService.GetConnectedConsole())
            {
                _ToBeRemove.Add(station);
            }
        }

        private void HealthCheck()
        {
            if (_ActivateHealthCheck)
            {
                InitToBeRemove();

                //Have a timer that will broadcast message to connected console and expect reply within 5 second else disconnect
                System.Timers.Timer HealthBroadcastTimer = new System.Timers.Timer();
                HealthBroadcastTimer.Interval = 10000; //10sec
                HealthBroadcastTimer.Elapsed += delegate { HealthBroadcastTimeOut(); };
                HealthBroadcastTimer.AutoReset = false;
                HealthBroadcastTimer.Start();
            }
        }

        private void HealthBroadcastTimeOut()
        {
            if (_ActivateHealthCheck)
            {
                //Broadcast message to all connected console
                _CallOut_CodingService.RequestConnStatus();

                System.Timers.Timer HealthResponseTimer = new System.Timers.Timer();
                HealthResponseTimer.Interval = 5000; //5sec
                HealthResponseTimer.Elapsed += delegate { HealthResponseTimeOut(); };
                HealthResponseTimer.AutoReset = false;
                HealthResponseTimer.Start();
            }
        }

        private void HealthResponseTimeOut()
        {
            if (_ActivateHealthCheck)
            {
                //Disconnected if there is no response
                foreach (string station in _ToBeRemove)
                {
                    _CallOut_CodingService.ConsoleLeave(station);

                    //start a new thread to check against tracker for reference on station status
                    var t = new Thread(() => FailedPendingMsg(station));
                    t.Start();
                }

                //recall healthcheck
                HealthCheck();
            }
        }

        private void FailedPendingMsg(string station)
        {
            foreach (GatewayTracker gatewaytrack in _GatewayTrackerList)
            {
                string value = "";
                if (gatewaytrack.StationStatus.TryGetValue(station, out value))
                {
                    if (value.Equals("Pending"))
                    {
                        TargetTimeout(station, gatewaytrack.CodingID, gatewaytrack.DispatchUnits.ToArray());
                    }
                }
            }
        }

        #endregion

        #region Coding Tab 

        private void CreateTestCodingEntry(CodingIncidentMessage testIncidentMsg, string pendingno)
        {
            CodingStatus codingstatus = new CodingStatus();
            codingstatus.IncidentID = testIncidentMsg.IncidentNo;
            codingstatus.CodingID = testIncidentMsg.CodingID;
            codingstatus.Received = ""; //received from CAD
            codingstatus.Updated = ""; //received from Console
            codingstatus.Pending = pendingno;
            codingstatus.Acknowledged = "0";
            codingstatus.Rejected = "0";
            codingstatus.Failed = "0";

            _CodingStatusList.Add(codingstatus);
        }

        private void CreateCodingEntry(CodingIncidentMessage codingIncidentMsg, string pendingno)
        {
            CodingStatus codingstatus = new CodingStatus();
            codingstatus.IncidentID = codingIncidentMsg.IncidentNo;
            codingstatus.CodingID = codingIncidentMsg.CodingID;
            DateTime currentdt = DateTime.Now;
            codingstatus.Received = String.Format("{0:g}", currentdt); //received from CAD
            codingstatus.Updated = ""; //received from Console
            codingstatus.Pending = pendingno;
            codingstatus.Acknowledged = "0";
            codingstatus.Rejected = "0";
            codingstatus.Failed = "0";

            _CodingStatusList.Add(codingstatus);
        }

        public void UpdateCodingStatus(string codingID, string status)
        {
            foreach (CodingStatus codingstatus in _CodingStatusList)
            {
                if (codingstatus.CodingID.Equals(codingID))
                {
                    //Minus from pending
                    int PendingNo = Int32.Parse(codingstatus.Pending);
                    PendingNo--;
                    codingstatus.Pending = PendingNo.ToString();

                    //Adding to Ack/Reject/Failed
                    if (status.Equals("Acknowledged"))
                    {
                        int AckNo = Int32.Parse(codingstatus.Acknowledged);
                        AckNo++;
                        codingstatus.Acknowledged = AckNo.ToString();
                    }
                    else if (status.Equals("Rejected"))
                    {
                        int RejectNo = Int32.Parse(codingstatus.Rejected);
                        RejectNo++;
                        codingstatus.Rejected = RejectNo.ToString();
                    }
                    else
                    {
                        int FailedNo = Int32.Parse(codingstatus.Failed);
                        FailedNo++;
                        codingstatus.Failed = FailedNo.ToString();
                    }

                    //Update the updated timestamp from Console
                    DateTime currentdt = DateTime.Now;
                    codingstatus.Updated = String.Format("{0:g}", currentdt);
                }
            }
        }

        #region Initialise Coding Data Grid View

        private void InitCodingDataGrid()
        {
            //Initialize the datagridview
            this.dgvCoding.AutoGenerateColumns = false;
            this.dgvCoding.CellBorderStyle = DataGridViewCellBorderStyle.None;

            DataGridViewTextBoxColumn incidentIDColumn = new DataGridViewTextBoxColumn();
            incidentIDColumn.DataPropertyName = "IncidentID";
            incidentIDColumn.HeaderText = "IncidentID";
            incidentIDColumn.ReadOnly = true;
            incidentIDColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

            DataGridViewTextBoxColumn codingIDColumn = new DataGridViewTextBoxColumn();
            codingIDColumn.DataPropertyName = "CodingID";
            codingIDColumn.HeaderText = "CodingID";
            codingIDColumn.ReadOnly = true;
            codingIDColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

            DataGridViewTextBoxColumn receivedColumn = new DataGridViewTextBoxColumn();
            receivedColumn.DataPropertyName = "Received";
            receivedColumn.HeaderText = "Received";
            receivedColumn.ReadOnly = true;
            receivedColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

            DataGridViewTextBoxColumn updatedColumn = new DataGridViewTextBoxColumn();
            updatedColumn.DataPropertyName = "Updated";
            updatedColumn.HeaderText = "Updated";
            updatedColumn.ReadOnly = true;
            updatedColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

            DataGridViewTextBoxColumn pendingColumn = new DataGridViewTextBoxColumn();
            pendingColumn.DataPropertyName = "Pending";
            pendingColumn.HeaderText = "Pending";
            pendingColumn.ReadOnly = true;
            pendingColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

            DataGridViewTextBoxColumn ackColumn = new DataGridViewTextBoxColumn();
            ackColumn.DataPropertyName = "Acknowledged";
            ackColumn.HeaderText = "Acknowledged";
            ackColumn.ReadOnly = true;
            ackColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

            DataGridViewTextBoxColumn rejectedColumn = new DataGridViewTextBoxColumn();
            rejectedColumn.DataPropertyName = "Rejected";
            rejectedColumn.HeaderText = "Rejected";
            rejectedColumn.ReadOnly = true;
            rejectedColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

            DataGridViewTextBoxColumn failedColumn = new DataGridViewTextBoxColumn();
            failedColumn.DataPropertyName = "Failed";
            failedColumn.HeaderText = "Failed";
            failedColumn.ReadOnly = true;
            failedColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

            this.dgvCoding.Columns.Add(incidentIDColumn);
            this.dgvCoding.Columns.Add(codingIDColumn);
            this.dgvCoding.Columns.Add(receivedColumn);
            this.dgvCoding.Columns.Add(updatedColumn);
            this.dgvCoding.Columns.Add(pendingColumn);
            this.dgvCoding.Columns.Add(ackColumn);
            this.dgvCoding.Columns.Add(rejectedColumn);
            this.dgvCoding.Columns.Add(failedColumn);

            _CodingStatusList.Clear();
            this.dgvCoding.DataSource = _CodingStatusList;
        }

        #endregion

        #endregion

        #region Message Tab

        private void CreateMessageEntry(CodingIncidentMessage codingIncidentMsg, string acktotal)
        {
            MessageStatus messagestatus = new MessageStatus();
            messagestatus.CodingID = codingIncidentMsg.CodingID;
            DateTime currentdt = DateTime.Now;
            messagestatus.AckTimeStamp = String.Format("{0:g}", currentdt);
            messagestatus.AckFrom = "Gateway";
            messagestatus.AckStatus = "Pending";
            messagestatus.AckNo = "0";
            messagestatus.AckTotal = acktotal;

            _MessageStatusList.Add(messagestatus);
        }

        public MessageStatus UpdateMessageStatus(string codingID, string station, string status)
        {
            int lastackno = 0;
            string acktotal = "";

            //Update Message Status
            foreach (MessageStatus messagestatus in _MessageStatusList)
            {
                if (messagestatus.CodingID.Equals(codingID))
                {
                    //find latest ack no
                    lastackno = Math.Max(lastackno, Int32.Parse(messagestatus.AckNo));

                    //get ackTotal
                    acktotal = messagestatus.AckTotal;
                }
            }

            //Create new entry base on the coding ack message
            MessageStatus newMsgStatus = new MessageStatus();
            newMsgStatus.CodingID = codingID;
            DateTime currentdt = DateTime.Now;
            newMsgStatus.AckTimeStamp = String.Format("{0:g}", currentdt);
            newMsgStatus.AckFrom = station;
            newMsgStatus.AckStatus = status;
            newMsgStatus.AckNo = (lastackno + 1).ToString();
            newMsgStatus.AckTotal = acktotal;

            _MessageStatusList.Add(newMsgStatus);

            //Update on gateway tracker
            foreach (GatewayTracker gatewaytrack in _GatewayTrackerList)
            {
                //if match codingID and station name, update the station status
                if (gatewaytrack.CodingID.Equals(codingID) && gatewaytrack.StationStatus.ContainsKey(station))
                {
                    gatewaytrack.StationStatus[station] = status;
                }
            }

            return newMsgStatus;

        }

        private bool CheckNoDelayMsg(string codingID, string console)
        {
            foreach (MessageStatus messagestatus in _MessageStatusList)
            {
                if (messagestatus.CodingID.Equals(codingID) && messagestatus.AckFrom.Equals(console))
                {
                    return false;
                }
            }
            return true;
        }

        #region Initialise Message Data Grid View

        private void InitMessageDataGrid()
        {
            //Initialize the datagridview
            this.dgvMessage.AutoGenerateColumns = false;
            this.dgvMessage.CellBorderStyle = DataGridViewCellBorderStyle.None;

            DataGridViewTextBoxColumn codingIDColumn = new DataGridViewTextBoxColumn();
            codingIDColumn.DataPropertyName = "CodingID";
            codingIDColumn.HeaderText = "CodingID";
            codingIDColumn.ReadOnly = true;
            codingIDColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

            DataGridViewTextBoxColumn acktimeColumn = new DataGridViewTextBoxColumn();
            acktimeColumn.DataPropertyName = "AckTimeStamp";
            acktimeColumn.HeaderText = "ackTimeStamp";
            acktimeColumn.ReadOnly = true;
            acktimeColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

            DataGridViewTextBoxColumn ackfromColumn = new DataGridViewTextBoxColumn();
            ackfromColumn.DataPropertyName = "AckFrom";
            ackfromColumn.HeaderText = "ackFrom";
            ackfromColumn.ReadOnly = true;
            ackfromColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

            DataGridViewTextBoxColumn ackstatusColumn = new DataGridViewTextBoxColumn();
            ackstatusColumn.DataPropertyName = "AckStatus";
            ackstatusColumn.HeaderText = "ackStatus";
            ackstatusColumn.ReadOnly = true;
            ackstatusColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

            DataGridViewTextBoxColumn ackNoColumn = new DataGridViewTextBoxColumn();
            ackNoColumn.DataPropertyName = "AckNo";
            ackNoColumn.HeaderText = "ackNo";
            ackNoColumn.ReadOnly = true;
            ackNoColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

            DataGridViewTextBoxColumn acktotalColumn = new DataGridViewTextBoxColumn();
            acktotalColumn.DataPropertyName = "AckTotal";
            acktotalColumn.HeaderText = "ackTotal";
            acktotalColumn.ReadOnly = true;
            acktotalColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

            this.dgvMessage.Columns.Add(codingIDColumn);
            this.dgvMessage.Columns.Add(acktimeColumn);
            this.dgvMessage.Columns.Add(ackfromColumn);
            this.dgvMessage.Columns.Add(ackstatusColumn);
            this.dgvMessage.Columns.Add(ackNoColumn);
            this.dgvMessage.Columns.Add(acktotalColumn);

            _MessageStatusList.Clear();

            this.dgvMessage.DataSource = _MessageStatusList;
        }

        #endregion

        #endregion

        #region Station Tab

        private void btnStationTarget_Click(object sender, EventArgs e)
        {
            //Find Station that is been checked
            List<string> tmpstationList = new List<string>();
            foreach (DataGridViewRow row in dgvStation.Rows)
            {
                if (dgvStation.Rows[row.Index].Cells[3].Value != null)
                {
                    if ((bool)dgvStation.Rows[row.Index].Cells[3].Value)
                    {
                        tmpstationList.Add(dgvStation.Rows[row.Index].Cells[1].Value.ToString()); //add station name
                    }
                }
            }
            string[] addressList = tmpstationList.ToArray();

            CodingIncidentMessage testIncidentMsg = TestMessageTemplate();
            ConvertCodingtoTracker(addressList, testIncidentMsg); //Add to gateway tracker
            Log("Send target test message");
            _CallOut_CodingService.TargetMsg(addressList, testIncidentMsg);

            List<Tracking> trackingList = new List<Tracking>();
            //Only take out the stations on the Current Station
            foreach (string station in tmpstationList)
            {
                Tracking newstation = new Tracking();
                newstation.Station = station;
                newstation.Status = "Pending";

                List<string> unitcallsign = new List<string>();

                //To give relevant station units callsign
                foreach (CodingUnits unit in testIncidentMsg.DispatchUnits)
                {
                    if (unit.UnitCurrentStation.Equals(station))
                    {
                        unitcallsign.Add(unit.Callsign);
                    }

                    //For test message
                    if (unit.UnitCurrentStation.Equals("Test"))
                    {
                        unitcallsign.Add(unit.Callsign);
                    }
                }
                newstation.Unit = unitcallsign.ToArray();
                trackingList.Add(newstation); //Add into tracking list
            }

            //Send ack back to CAD
            CADIncidentAck cadincidentack = new CADIncidentAck();
            cadincidentack.CodingID = testIncidentMsg.CodingID;
            cadincidentack.AckTracking = trackingList.ToArray();
            DateTime currentdt = DateTime.Now;
            cadincidentack.AckTimeStamp = currentdt;
            cadincidentack.AckNo = 0;
            cadincidentack.AckTotal = tmpstationList.Count;

            Log("Send Ack to CAD for target test message");
            _CallOut_CADService.AckCADIncidentMsg(cadincidentack);

            //Set Coding Entry
            CreateTestCodingEntry(testIncidentMsg, tmpstationList.Count.ToString());
            //Set Message Entry
            CreateMessageEntry(testIncidentMsg, tmpstationList.Count.ToString());
        }

        private void btnStationBroadcast_Click(object sender, EventArgs e)
        {
            //Find Station that is connected
            List<string> connectedConsole = new List<string>();
            foreach (string console in _CallOut_CodingService.GetConnectedConsole())
            {
                connectedConsole.Add(console);
            }
            string[] addressList = connectedConsole.ToArray();

            CodingIncidentMessage testIncidentMsg = TestMessageTemplate2();
            ConvertCodingtoTracker(addressList, testIncidentMsg); //Add to gateway tracker
            Log("Send broadcast test message");
            _CallOut_CodingService.TargetMsg(addressList, testIncidentMsg);

            List<Tracking> trackingList = new List<Tracking>();
            //Only take out the stations on the Current Station
            foreach (string station in connectedConsole)
            {
                Tracking newstation = new Tracking();
                newstation.Station = station;
                newstation.Status = "Pending";

                List<string> unitcallsign = new List<string>();

                //To give relevant station units callsign
                foreach (CodingUnits unit in testIncidentMsg.DispatchUnits)
                {
                    if (unit.UnitCurrentStation.Equals(station))
                    {
                        unitcallsign.Add(unit.Callsign);
                    }

                    //For test message
                    if (unit.UnitCurrentStation.Equals("Test"))
                    {
                        unitcallsign.Add(unit.Callsign);
                    }
                }
                newstation.Unit = unitcallsign.ToArray();
                trackingList.Add(newstation); //Add into tracking list
            }

            //Send ack back to CAD
            CADIncidentAck cadincidentack = new CADIncidentAck();
            cadincidentack.CodingID = testIncidentMsg.CodingID;
            cadincidentack.AckTracking = trackingList.ToArray();
            DateTime currentdt = DateTime.Now;
            cadincidentack.AckTimeStamp = currentdt;
            cadincidentack.AckNo = 0;
            cadincidentack.AckTotal = connectedConsole.Count;

            Log("Send Ack to CAD for broadcast test message");
            _CallOut_CADService.AckCADIncidentMsg(cadincidentack);

            //Set Coding Entry
            CreateTestCodingEntry(testIncidentMsg, connectedConsole.Count.ToString());
            //Set Message Entry
            CreateMessageEntry(testIncidentMsg, connectedConsole.Count.ToString());
        }

        private CodingIncidentMessage TestMessageTemplate()
        {
            string testID = _CallOut_CodingService.GetTestID();
            string testNo = testID;
            string testTitle = this.txtTestMsg.Text; //Test Message

            CodingLocation testlocation = new CodingLocation();
            testlocation.Name = "Test Location";
            testlocation.Street = "Test Address";
            testlocation.Unit = "Test Unit";
            testlocation.State = "Test State";
            testlocation.City = "Test City";
            testlocation.Country = "Test Country";
            testlocation.PostalCode = "123456";

            string testType = "Test Type";
            int testAlarm = 0;
            string testPriority = "0";
            DateTime testDateTime = DateTime.Now;

            CodingUnits testUnit1 = new CodingUnits();
            testUnit1.ID = 123;
            testUnit1.Callsign = "T100";
            testUnit1.UnitType = "Test Unit 1";
            testUnit1.FromStatus = "";
            testUnit1.UnitLocation = "";
            testUnit1.UnitHomeStation = "";
            testUnit1.UnitCurrentStation = "Test";

            CodingUnits testUnit2 = new CodingUnits();
            testUnit1.ID = 456;
            testUnit2.Callsign = "T200";
            testUnit2.UnitType = "Test Unit 2";
            testUnit2.FromStatus = "";
            testUnit2.UnitLocation = "";
            testUnit2.UnitHomeStation = "";
            testUnit2.UnitCurrentStation = "Test";

            CodingUnits[] testUnitList = new CodingUnits[2];
            testUnitList[0] = testUnit1;
            testUnitList[1] = testUnit2;

            CodingIncidentMessage testIncidentMsg = new CodingIncidentMessage();
            testIncidentMsg.CodingID = testID;
            testIncidentMsg.IncidentNo = testNo;
            testIncidentMsg.IncidentTitle = testTitle;
            testIncidentMsg.IncidentLocation = testlocation;
            testIncidentMsg.IncidentType = testType;
            testIncidentMsg.IncidentAlarm = testAlarm;
            testIncidentMsg.IncidentPriority = testPriority;
            testIncidentMsg.DispatchDateTime = testDateTime;
            testIncidentMsg.DispatchUnits = testUnitList;

            return testIncidentMsg;
        }

        private CodingIncidentMessage TestMessageTemplate2()
        {
            string testID = _CallOut_CodingService.GetTestID();
            string testNo = testID;
            string testTitle = this.txtTestMsg.Text; //Test Message

            CodingLocation testlocation = new CodingLocation();
            testlocation.Name = "Test Location";
            testlocation.Street = "Test Address";
            testlocation.Unit = "Test Unit";
            testlocation.State = "Test State";
            testlocation.City = "Test City";
            testlocation.Country = "Test Country";
            testlocation.PostalCode = "123456";

            string testType = "Test Type";
            int testAlarm = 0;
            string testPriority = "0";
            DateTime testDateTime = DateTime.Now;

            CodingUnits testUnit1 = new CodingUnits();
            testUnit1.ID = 123;
            testUnit1.Callsign = "T100";
            testUnit1.UnitType = "Test Unit 1";
            testUnit1.FromStatus = "";
            testUnit1.UnitLocation = "";
            testUnit1.UnitHomeStation = "";
            testUnit1.UnitCurrentStation = "Test";

            CodingUnits testUnit2 = new CodingUnits();
            testUnit1.ID = 456;
            testUnit2.Callsign = "T200";
            testUnit2.UnitType = "Test Unit 2";
            testUnit2.FromStatus = "";
            testUnit2.UnitLocation = "";
            testUnit2.UnitHomeStation = "";
            testUnit2.UnitCurrentStation = "Test";

            CodingUnits testUnit3 = new CodingUnits();
            testUnit3.ID = 123;
            testUnit3.Callsign = "T300";
            testUnit3.UnitType = "Test Unit 3";
            testUnit3.FromStatus = "";
            testUnit3.UnitLocation = "";
            testUnit3.UnitHomeStation = "";
            testUnit3.UnitCurrentStation = "Test";

            CodingUnits testUnit4 = new CodingUnits();
            testUnit4.ID = 456;
            testUnit4.Callsign = "T400";
            testUnit4.UnitType = "Test Unit 4";
            testUnit4.FromStatus = "";
            testUnit4.UnitLocation = "";
            testUnit4.UnitHomeStation = "";
            testUnit4.UnitCurrentStation = "Test";

            CodingUnits[] testUnitList = new CodingUnits[4];
            testUnitList[0] = testUnit1;
            testUnitList[1] = testUnit2;
            testUnitList[2] = testUnit3;
            testUnitList[3] = testUnit4;

            CodingIncidentMessage testIncidentMsg = new CodingIncidentMessage();
            testIncidentMsg.CodingID = testID;
            testIncidentMsg.IncidentNo = testNo;
            testIncidentMsg.IncidentTitle = testTitle;
            testIncidentMsg.IncidentLocation = testlocation;
            testIncidentMsg.IncidentType = testType;
            testIncidentMsg.IncidentAlarm = testAlarm;
            testIncidentMsg.IncidentPriority = testPriority;
            testIncidentMsg.DispatchDateTime = testDateTime;
            testIncidentMsg.DispatchUnits = testUnitList;

            return testIncidentMsg;
        }

        #region Initialise Station Data Grid View

        private void InitStationDataGrid()
        {
            //Initialize the datagridview
            this.dgvStation.AutoGenerateColumns = false;
            this.dgvStation.CellBorderStyle = DataGridViewCellBorderStyle.None;
            this.dgvStation.CellPainting += new DataGridViewCellPaintingEventHandler(dgvStation_CellPainting);
            this.dgvStation.CellValueChanged += new DataGridViewCellEventHandler(dgvStation_CellValueChanged);

            DataGridViewTextBoxColumn statusColumn = new DataGridViewTextBoxColumn();
            statusColumn.DataPropertyName = "Status";
            statusColumn.HeaderText = "Status";
            statusColumn.ReadOnly = true;
            statusColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

            DataGridViewTextBoxColumn stationColumn = new DataGridViewTextBoxColumn();
            stationColumn.DataPropertyName = "Station";
            stationColumn.HeaderText = "Station";
            stationColumn.ReadOnly = true;
            stationColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

            DataGridViewTextBoxColumn updateColumn = new DataGridViewTextBoxColumn();
            updateColumn.DataPropertyName = "Update";
            updateColumn.HeaderText = "Latest Update";
            updateColumn.ReadOnly = true;
            updateColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

            DataGridViewCheckBoxColumn targetColumn = new DataGridViewCheckBoxColumn();
            targetColumn.DataPropertyName = "Target";
            targetColumn.Name = "Target";
            targetColumn.HeaderText = "Target";
            targetColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            targetColumn.ThreeState = false;

            this.dgvStation.Columns.Add(statusColumn);
            this.dgvStation.Columns.Add(stationColumn);
            this.dgvStation.Columns.Add(updateColumn);
            this.dgvStation.Columns.Add(targetColumn);

            _StationStatusList.Clear();
            //foreach (StationStatus station in _CallOut_CodingService.GetStationStatus())
            //{
            //    _StationStatusList.Add(station);
            //}
            foreach (StationStatus station in _CallOut_CodingService.GetStationStatus())
            {
                _StationStatusList.Add(station);
            }

            this.dgvStation.DataSource = _StationStatusList;
        }

        //Paint over to cover the checkbox if status is offline
        private void dgvStation_CellPainting(object sender, System.Windows.Forms.DataGridViewCellPaintingEventArgs e)
        {
            // Set the checkbox column
            if (this.dgvStation.Columns[3].Index == e.ColumnIndex && e.RowIndex >= 0)
            {
                // If only it is offline
                if (this.dgvStation[0, e.RowIndex].Value.ToString() == "Offline")
                {
                    // You can change e.CellStyle.BackColor to Color of your background
                    using (Brush backColorBrush = new SolidBrush(e.CellStyle.BackColor))
                    {
                        // Erase the cell.
                        e.Graphics.FillRectangle(backColorBrush, e.CellBounds);
                        e.Handled = true;
                    }

                    //Set read only to prevent from accident editing
                    this.dgvStation[3, e.RowIndex].ReadOnly = true;
                }
                else //When Online release read only condition
                {
                    this.dgvStation[3, e.RowIndex].ReadOnly = false;
                }
            }
        }

        //Refresh paint once there is changes in value at status column
        private void dgvStation_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 0)
            {
                dgvStation.InvalidateCell(3, e.RowIndex);
            }
        }

        #endregion

        #endregion

        #region CallOut_CodingServiceCallback Methods

        public void GatewayRcvConnStatus(string console)
        {
            SendOrPostCallback callback =
                delegate (object state)
                {
                    //remove station that had reply from the list
                    _ToBeRemove.Remove(console);
                };

            _uiSyncContext.Post(callback, "rcv conn status reply");
        }

        public void EditConnStatus(string StationName, string Status)
        {
            SendOrPostCallback callback =
                delegate (object state)
                {
                    //To toggle the status 
                    foreach (StationStatus station in _StationStatusList)
                    {
                        if (station.Station.Equals(StationName))
                        {
                            station.Status = Status;
                            DateTime currentdt = DateTime.Now;
                            station.Update = String.Format("{0:g}", currentdt);
                            _CallOut_CodingService.UpdateStationStatus(station.Status, station.Station, station.Update);

                            //Auto Uncheck the checkbox when offline
                            if (Status == "Offline")
                            {
                                foreach (DataGridViewRow row in dgvStation.Rows)
                                {
                                    if (dgvStation.Rows[row.Index].Cells[1].Value.ToString() == StationName)
                                    {
                                        //exit from edit mode so value will be received
                                        dgvStation.EndEdit();
                                        dgvStation.Rows[row.Index].Cells[3].Value = false;
                                    }
                                }
                            }
                        }
                    }
                    this.dgvStation.Refresh();
                };

            _uiSyncContext.Post(callback, "Edit Connection Status");

        }

        //Notify gateway that deliever of message failed due to console not online (instant failed)
        public void NotifyConsoleNotConnected(string console, CodingIncidentMessage codingIncidentMsg)
        {
            SendOrPostCallback callback =
                delegate (object state)
                {
                    if (CheckNoDelayMsg(codingIncidentMsg.CodingID, console))
                    {
                        //Updated coding status failed
                        UpdateCodingStatus(codingIncidentMsg.CodingID, "Failed");

                        //Updated message status failed
                        MessageStatus messagestatus = UpdateMessageStatus(codingIncidentMsg.CodingID, console, "Failed");

                        List<string> unitcallsign = new List<string>();
                        //To give relevant station units callsign
                        foreach (CodingUnits unit in codingIncidentMsg.DispatchUnits)
                        {
                            if (unit.UnitCurrentStation.Equals(console))
                            {
                                unitcallsign.Add(unit.Callsign);
                            }
                        }

                        //Broadcast Coding status back to CAD (failed)
                        SendBroadcastIncidentCoding(console, "Failed", unitcallsign.ToArray(), messagestatus);

                        //Update on gateway tracker
                        foreach (GatewayTracker gatewaytrack in _GatewayTrackerList)
                        {
                            //if match codingID and station name, update the station status
                            if (gatewaytrack.CodingID.Equals(codingIncidentMsg.CodingID) && gatewaytrack.StationStatus.ContainsKey(console))
                            {
                                gatewaytrack.StationStatus[console] = "Failed";
                            }
                        }
                    }
                };

            _uiSyncContext.Post(callback, "Notify deliever failed");
        }

        public void RcvCodingAckMsg(CodingAckMessage codingAckMsg)
        {
            SendOrPostCallback callback =
                delegate (object state)
                {
                    if (CheckNoDelayMsg(codingAckMsg.CodingID, codingAckMsg.ConsoleID))
                    {
                        //Update Coding Status
                        UpdateCodingStatus(codingAckMsg.CodingID, codingAckMsg.AckStatus);

                        //Update Message Status
                        MessageStatus messagestatus = UpdateMessageStatus(codingAckMsg.CodingID, codingAckMsg.ConsoleID, codingAckMsg.AckStatus);

                        //Update Station Status
                        foreach (StationStatus stationstatus in _StationStatusList)
                        {
                            if (stationstatus.Station.Equals(codingAckMsg.ConsoleID))
                            {
                                stationstatus.Update = codingAckMsg.AckTimeStamp;
                            }
                        }

                        this.dgvStation.Refresh();

                        //Broadcast Coding status back to CAD (Ack,reject,timeout/failed)
                        SendBroadcastIncidentCoding(codingAckMsg.ConsoleID, codingAckMsg.AckStatus, codingAckMsg.AckUnits, messagestatus);
                        //for test at 020715
                        //CallAPI(codingAckMsg.IncidentNo, codingAckMsg.AckStatus);
                    }
                };

            _uiSyncContext.Post(callback, "updated Coding Ack Msg");

        }

        public void SendBroadcastIncidentCoding(string console, string status, string[] unitcallsign, MessageStatus messagestatus)
        {
            //Get Station tracking
            Tracking stationTrack = new Tracking();
            stationTrack.Station = console;
            stationTrack.Status = status;
            stationTrack.Unit = unitcallsign;

            //Broadcast Coding status back to CAD (failed)
            CADIncidentCodingStatus incidentcodingstatus = new CADIncidentCodingStatus();
            incidentcodingstatus.CodingID = messagestatus.CodingID;
            incidentcodingstatus.AckTracking = stationTrack;
            incidentcodingstatus.AckFrom = messagestatus.AckFrom;
            incidentcodingstatus.AckStatus = messagestatus.AckStatus;
            DateTime currentdt = DateTime.Now;
            incidentcodingstatus.AckTimeStamp = currentdt;
            incidentcodingstatus.AckNo = Int32.Parse(messagestatus.AckNo);
            incidentcodingstatus.AckTotal = Int32.Parse(messagestatus.AckTotal);
            //incidentcodingstatus.AckTotal = string.IsNullOrEmpty(messagestatus.AckTotal) ? 0 : Int32.Parse(messagestatus.AckTotal);
            Log("Broadcast status back to CAD");
            _CallOut_CADService.BroadcastIncidentCodingStatus(incidentcodingstatus);
        }

        public void StartTargetTimeoutTimer(string console, CodingIncidentMessage codingIncidentMsg)
        {
            SendOrPostCallback callback =
                delegate (object state)
                {
                    //Start timer for 35 sec to assume console disconnected
                    System.Timers.Timer AutoFailedTimer = new System.Timers.Timer();
                    AutoFailedTimer.Interval = 35000; //35 sec
                    AutoFailedTimer.Elapsed += delegate { TargetTimeout(console, codingIncidentMsg.CodingID, codingIncidentMsg.DispatchUnits); };
                    AutoFailedTimer.AutoReset = false;
                    AutoFailedTimer.Start();
                };

            _uiSyncContext.Post(callback, "start target timeout timer");
        }

        public void TargetTimeout(string console, string codingID, CodingUnits[] dispatchUnits)
        {
            //Debug.WriteLine("TargetTimeout");
            //Cross thread 
            SendOrPostCallback callback = delegate (object state)
            {
                if (CheckNoDelayMsg(codingID, console))
                {
                    //Check against Pending number in coding tab, If there is any pending
                    //remove those which had respond base on message tab with codingID
                    foreach (CodingStatus codingstatus in _CodingStatusList)
                    {
                        if (codingstatus.CodingID.Equals(codingID))
                        {
                            if (codingstatus.Pending != "0")
                            {
                                //Remove those station that already reply in the list
                                bool nomatchinmsgstatus = true;
                                foreach (MessageStatus msgstatus in _MessageStatusList)
                                {
                                    if (msgstatus.CodingID.Equals(codingID) && msgstatus.AckFrom.Equals(console))
                                    {
                                        nomatchinmsgstatus = false; //console already acknowledge
                                    }
                                }

                                if (nomatchinmsgstatus)
                                {
                                    //Notify those failed msg to console
                                    _CallOut_CodingService.RemovefromMsgQueue(console, codingID);

                                    //update coding status
                                    UpdateCodingStatus(codingID, "Failed");
                                    //update message status
                                    MessageStatus messagestatus = UpdateMessageStatus(codingID, console, "Failed");

                                    List<string> unitcallsign = new List<string>();
                                    //To give relevant station units callsign
                                    foreach (CodingUnits unit in dispatchUnits)
                                    {
                                        if (unit.UnitCurrentStation.Equals(console))
                                        {
                                            unitcallsign.Add(unit.Callsign);
                                        }

                                        //For Test message
                                        if (unit.UnitCurrentStation.Equals("Test"))
                                        {
                                            unitcallsign.Add(unit.Callsign);
                                        }
                                    }

                                    //Broadcast Coding status back to CAD (failed)
                                    SendBroadcastIncidentCoding(console, "Failed", unitcallsign.ToArray(), messagestatus);
                                }
                            }
                        }
                    }
                }
            };

            _uiSyncContext.Post(callback, "target timeout");
        }

        #region Methods not for Gateway

        public void ConsoleDisplayMsg(CodingIncidentMessage codingIncidentMsg)
        { }
        public void ConsoleRcvConnStatus()
        { }
        public void UpdateRemoveMsgList(string CodingID)
        { }

        #endregion

        #endregion

        #region CallOut_CADServiceCallback Methods

        /* 
         * 1) Convert from incident message into coding message
         * 2) Send the coding message to relevant console 
         * 3) Send Ack back to CAD in order to update mainly codingID
         */
        public void RcvCADIncidentMsg(ServiceReference2.DispatchedIncident CADincidentMsg)
        {
            SendOrPostCallback callback =
                delegate (object state)
                {
                    List<string> tmpstationList = new List<string>();
                    List<Tracking> trackingList = new List<Tracking>();
                    //Only take out the stations on the Current Station
                    foreach (ServiceReference2.DispatchedUnit uniqueunit in CADincidentMsg.ListOfUnits)
                    {
                        //Avoid duplicate station name in the list
                        if (!tmpstationList.Contains(uniqueunit.CurrentStation))
                        {
                            tmpstationList.Add(uniqueunit.CurrentStation);
                            Tracking newstation = new Tracking();
                            newstation.Station = uniqueunit.CurrentStation;
                            newstation.Status = "Pending";

                            List<string> unitcallsign = new List<string>();

                            //To give relevant station units callsign
                            foreach (ServiceReference2.DispatchedUnit unit in CADincidentMsg.ListOfUnits)
                            {
                                if (unit.CurrentStation.Equals(uniqueunit.CurrentStation))
                                {
                                    unitcallsign.Add(unit.CallSign);
                                }
                            }

                            newstation.Unit = unitcallsign.ToArray();
                            trackingList.Add(newstation); //Add into tracking list
                        }
                    }

                    string[] addressList = tmpstationList.ToArray();

                    //Convert Incident to Coding Message and send to respective console
                    CodingIncidentMessage codingincidentmsg = ConvertIncidentToCoding(CADincidentMsg);
                    ConvertCodingtoTracker(addressList, codingincidentmsg); //Add to gateway tracker
                    Log("Forward message from simulate CAD to respective console");
                    _CallOut_CodingService.TargetMsg(addressList, codingincidentmsg);

                    //Send ack back to CAD
                    CADIncidentAck cadincidentack = new CADIncidentAck();
                    cadincidentack.CodingID = codingincidentmsg.CodingID;
                    cadincidentack.AckTracking = trackingList.ToArray();
                    DateTime currentdt = DateTime.Now;
                    cadincidentack.AckTimeStamp = currentdt;
                    cadincidentack.AckNo = 0;
                    cadincidentack.AckTotal = tmpstationList.Count;
                    Log("Send ack to simulate CAD for forward message to console");
                    _CallOut_CADService.AckCADIncidentMsg(cadincidentack);


                    //Set Coding Entry
                    CreateCodingEntry(codingincidentmsg, tmpstationList.Count.ToString());
                    //Set Message Entry
                    CreateMessageEntry(codingincidentmsg, tmpstationList.Count.ToString());

                };

            _uiSyncContext.Post(callback, "Rcv Incident Message");
        }

        public CodingIncidentMessage ConvertIncidentToCoding(ServiceReference2.DispatchedIncident CADincidentMsg)
        {
            string codingID = _CallOut_CodingService.GetCodingID();

            CodingLocation incidentLocation = new CodingLocation();
            incidentLocation.Name = CADincidentMsg.IncidentLocation.Name;
            incidentLocation.Street = CADincidentMsg.IncidentLocation.Street;
            incidentLocation.Unit = CADincidentMsg.IncidentLocation.Unit;
            incidentLocation.State = CADincidentMsg.IncidentLocation.State;
            incidentLocation.City = CADincidentMsg.IncidentLocation.City;
            incidentLocation.Country = CADincidentMsg.IncidentLocation.Country;
            incidentLocation.PostalCode = CADincidentMsg.IncidentLocation.PostalCode;

            List<CodingUnits> tmpcodingunitList = new List<CodingUnits>();
            foreach (ServiceReference2.DispatchedUnit unit in CADincidentMsg.ListOfUnits)
            {
                CodingUnits newUnit = new CodingUnits();
                newUnit.ID = unit.ID;
                newUnit.Callsign = unit.CallSign;
                newUnit.UnitType = unit.UnitType;
                newUnit.FromStatus = unit.Status;
                newUnit.UnitLocation = unit.Location;
                newUnit.UnitHomeStation = unit.HomeStation;
                newUnit.UnitCurrentStation = unit.CurrentStation;

                tmpcodingunitList.Add(newUnit);
            }

            CodingUnits[] dispatchUnits = tmpcodingunitList.ToArray();

            CodingIncidentMessage codingIncidentMsg = new CodingIncidentMessage();
            codingIncidentMsg.CodingID = codingID;
            codingIncidentMsg.IncidentNo = CADincidentMsg.IncidentNumber;
            codingIncidentMsg.IncidentTitle = CADincidentMsg.IncidentTitle;
            codingIncidentMsg.IncidentLocation = incidentLocation;
            codingIncidentMsg.IncidentType = CADincidentMsg.IncidentType;
            codingIncidentMsg.IncidentAlarm = CADincidentMsg.AlarmLevel;
            codingIncidentMsg.IncidentPriority = CADincidentMsg.Priority;
            codingIncidentMsg.DispatchDateTime = CADincidentMsg.DateTime;
            codingIncidentMsg.DispatchUnits = dispatchUnits;

            return codingIncidentMsg;
        }

        //Convert the coding incident message to gateway tracker and add to the tracker list for reference of information
        public void ConvertCodingtoTracker(string[] addressList, CodingIncidentMessage codingincidentMsg)
        {
            GatewayTracker gatewaytrack = new GatewayTracker();
            gatewaytrack.CodingID = codingincidentMsg.CodingID;
            gatewaytrack.IncidentID = codingincidentMsg.IncidentNo;

            List<CodingUnits> dispatchunits = new List<CodingUnits>();
            foreach (CodingUnits unit in codingincidentMsg.DispatchUnits)
            {
                dispatchunits.Add(unit);
            }

            Dictionary<string, string> stationstatus = new Dictionary<string, string>();
            foreach (string station in addressList)
            {
                stationstatus.Add(station, "Pending");
            }

            gatewaytrack.DispatchUnits = dispatchunits;
            gatewaytrack.StationStatus = stationstatus;

            _GatewayTrackerList.Add(gatewaytrack);
        }

        //Retrieve Incident Coding Status base on Query
        public void IncidentCodingStatus(string querycodingID)
        {
            SendOrPostCallback callback =
                delegate (object state)
                {
                    //Gather incident coding status base on coding ID
                    int acktotal = 0;
                    int pendingno = 0;
                    DateTime currentdt = DateTime.Now;
                    bool validCodingID = false;

                    //Update coding status (received time)
                    foreach (CodingStatus codingstatus in _CodingStatusList)
                    {
                        if (codingstatus.CodingID.Equals(querycodingID))
                        {
                            //There is such codingID exist
                            validCodingID = true;

                            //Update the updated timestamp from Console
                            codingstatus.Updated = String.Format("{0:g}", currentdt);
                            acktotal = Int32.Parse(codingstatus.Pending) + Int32.Parse(codingstatus.Acknowledged)
                                + Int32.Parse(codingstatus.Rejected) + Int32.Parse(codingstatus.Failed);
                            pendingno = Int32.Parse(codingstatus.Pending);
                        }
                    }

                    if (validCodingID)
                    {
                        //Update message status (-1 in ack No)
                        //Create new entry base on the coding ack message
                        MessageStatus newMsgStatus = new MessageStatus();
                        newMsgStatus.CodingID = querycodingID;
                        newMsgStatus.AckTimeStamp = String.Format("{0:g}", currentdt);
                        newMsgStatus.AckFrom = "Gateway";
                        if (pendingno == 0)
                        {
                            newMsgStatus.AckStatus = "Completed";
                        }
                        else
                        {
                            newMsgStatus.AckStatus = "Pending";
                        }
                        newMsgStatus.AckNo = "-1";
                        newMsgStatus.AckTotal = acktotal.ToString();

                        _MessageStatusList.Add(newMsgStatus);

                        //Response back to CAD 
                        
                        List<Tracking> trackingList = new List<Tracking>();

                        foreach (GatewayTracker gatewaytrack in _GatewayTrackerList)
                        {
                            if (gatewaytrack.CodingID.Equals(querycodingID))
                            {
                                //Only take out the stations on the Current Station
                                foreach (KeyValuePair<string, string> station in gatewaytrack.StationStatus)
                                {
                                    Tracking newstation = new Tracking();
                                    newstation.Station = station.Key;
                                    newstation.Status = station.Value;

                                    List<string> unitcallsign = new List<string>();

                                    //To give relevant station units callsign
                                    foreach (CodingUnits unit in gatewaytrack.DispatchUnits)
                                    {
                                        if (unit.UnitCurrentStation.Equals(station.Key))
                                        {
                                            unitcallsign.Add(unit.Callsign);
                                        }
                                        
                                        //For Test message
                                        if (unit.UnitCurrentStation.Equals("Test"))
                                        {
                                            unitcallsign.Add(unit.Callsign);
                                        }
                                    }

                                    newstation.Unit = unitcallsign.ToArray();
                                    trackingList.Add(newstation); //Add into tracking list
                                }
                            }
                        }

                        CADIncidentAck codingQueryRsponse = new CADIncidentAck();
                        codingQueryRsponse.CodingID = newMsgStatus.CodingID;
                        codingQueryRsponse.AckTracking = trackingList.ToArray();
                        codingQueryRsponse.AckTimeStamp = currentdt;
                        codingQueryRsponse.AckNo = Int32.Parse(newMsgStatus.AckNo);
                        codingQueryRsponse.AckTotal = Int32.Parse(newMsgStatus.AckTotal);
                        Log("Respond to requested ad hoc request from simulate CAD");
                        _CallOut_CADService.IncidentCodingStatusResponse(codingQueryRsponse);
                    }
                };

            _uiSyncContext.Post(callback, "Request incident coding status");
        }

        #region Methods not for Gateway
        public void UpdateCADIncidentAck(CADIncidentAck CADincidentack)
        { }

        public void UpdateIncidentCodingStatus(CADIncidentCodingStatus incidentcodingstatus)
        { }

        public void RcvIncidentCodingStatusResponse(CADIncidentAck codingstatusresponse)
        { }
        #endregion

        #endregion

    }
}
