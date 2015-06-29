using System;
using System.Collections.Generic;
using System.ComponentModel; //for bindinglist
//using System.Data;
using System.Drawing;
using System.Threading;
using System.ServiceModel;
//using System.Linq;
//using System.Text;
using System.Windows.Forms;
using System.Diagnostics; //for debug
using System.Timers;

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
        private ServiceReference1.CallOut_CodingServiceClient _CallOut_CodingService = null;
        private ServiceReference2.CallOut_CADServiceClient _CallOut_CADService = null;

        private bool _isCADConnected = false;
        private bool _isConsoleConnected = false;

        //For binding of list to datagirdview
        //Bindinglist allow user to add row, however will left a empty last row.
        BindingList<CodingStatus> _CodingStatusList = new BindingList<CodingStatus>();
        BindingList<MessageStatus> _MessageStatusList = new BindingList<MessageStatus>();
        List<StationStatus> _StationStatusList = new List<StationStatus>();

        //List of informaton at is hold and track at gateway, e.g station status and dispatchunit info for adhoc request
        //pending and completed list?
        List<GatewayTracker> _GatewayTrackerList = new List<GatewayTracker>();

        //List of station that will be remove from service
        List<string> _ToBeRemove;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Capture the UI synchronization context
            _uiSyncContext = SynchronizationContext.Current;

            // The client callback interface must be hosted for the server to invoke the callback
            // Open a connection to the message service via the proxy (qualifier ServiceReference1 needed due to name clash)
            _CallOut_CodingService = new ServiceReference1.CallOut_CodingServiceClient(new InstanceContext(this), "NetTcpBinding_CallOut_CodingService");
            _CallOut_CodingService.Open();

            _CallOut_CADService = new ServiceReference2.CallOut_CADServiceClient(new InstanceContext(this), "WSDualHttpBinding_CallOut_CADService");
            _CallOut_CADService.Open();
            
            // Initial eventhandlers
            this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);

            //Set Up DataGridView
            InitCodingDataGrid();
            InitMessageDataGrid();
            InitStationDataGrid();

            //Init Health Checker aka check for ocnsole connectivity
            HealthCheck();

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //Terminate the connection to the service.
            if (_isConsoleConnected)
            {
                _CallOut_CodingService.Close();
            }
            if (_isCADConnected)
            {
                _CallOut_CADService.Close();
            }

        }

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
            InitToBeRemove();

            //Have a timer that will broadcast message to connected console and expect reply within 5 second else disconnect
            System.Timers.Timer HealthBroadcastTimer = new System.Timers.Timer();
            HealthBroadcastTimer.Interval = 10000; //10sec
            HealthBroadcastTimer.Elapsed += delegate { HealthBroadcastTimeOut(); };
            HealthBroadcastTimer.AutoReset = false;
            HealthBroadcastTimer.Start();
        }

        private void HealthBroadcastTimeOut()
        {
            //Broadcast message to all connected console
            _CallOut_CodingService.RequestConnStatus();

            System.Timers.Timer HealthResponseTimer = new System.Timers.Timer();
            HealthResponseTimer.Interval = 5000; //5sec
            HealthResponseTimer.Elapsed += delegate { HealthResponseTimeOut(); };
            HealthResponseTimer.AutoReset = false;
            HealthResponseTimer.Start();
        }

        private void HealthResponseTimeOut()
        {
            //Disconnected if there is no response
            foreach(string station in _ToBeRemove){
                _CallOut_CodingService.ConsoleLeave(station);

                //start a new thread to check against tracker for reference on station status
                var t = new Thread(() => FailedPendingMsg(station));
                t.Start();
            }

            //recall healthcheck
            HealthCheck();
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


        private void btnJoinCAD_Click(object sender, EventArgs e)
        {
            if (_isCADConnected)
            {
                // Let the service know that this user is leaving
                _CallOut_CADService.GatewayLeave();

                //Toggle button display
                _isCADConnected = false;
                this.btnJoinCAD.Text = "Join CAD";
            }
            else
            {
                //contact the service.
                _CallOut_CADService.GatewayJoin();

                //Toggle button display
                _isCADConnected = true;
                this.btnJoinCAD.Text = "Leave CAD";
            }
            
        }

        private void btnJoinConsole_Click(object sender, EventArgs e)
        {
            if (_isConsoleConnected)
            {
                // Let the service know that this user is leaving
                _CallOut_CodingService.GatewayLeave();

                //Toggle button display
                _isConsoleConnected = false;
                this.btnJoinConsole.Text = "Join Console";
            }
            else
            {
                //contact the service.
                _CallOut_CodingService.GatewayJoin();

                //Toggle button display
                _isConsoleConnected = true;
                this.btnJoinConsole.Text = "Leave Console";
            }
        }

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

        public void InitCodingDataGrid()
        {
            //Initialize the datagridview
            this.dgvCoding.AutoGenerateColumns = true;
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

        #region Initialise Message Data Grid View

        public void InitMessageDataGrid()
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
            //foreach (StationStatus station in _CallOut_CodingService.GetStationStatus())
            //{
            //    _MessageStatusList.Add(station);
            //}

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
            ConvertCodingtoTracker(testIncidentMsg); //Add to gateway tracker
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
            ConvertCodingtoTracker(testIncidentMsg); //Add to gateway tracker
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
                    if (unit.UnitCurrentStation.Equals(""))
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
            testUnit1.UnitCurrentStation = "";

            CodingUnits testUnit2 = new CodingUnits();
            testUnit1.ID = 456;
            testUnit2.Callsign = "T200";
            testUnit2.UnitType = "Test Unit 2";
            testUnit2.FromStatus = "";
            testUnit2.UnitLocation = "";
            testUnit2.UnitHomeStation = "";
            testUnit2.UnitCurrentStation = "";

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
            testUnit1.UnitCurrentStation = "";

            CodingUnits testUnit2 = new CodingUnits();
            testUnit1.ID = 456;
            testUnit2.Callsign = "T200";
            testUnit2.UnitType = "Test Unit 2";
            testUnit2.FromStatus = "";
            testUnit2.UnitLocation = "";
            testUnit2.UnitHomeStation = "";
            testUnit2.UnitCurrentStation = "";

            CodingUnits testUnit3 = new CodingUnits();
            testUnit3.ID = 123;
            testUnit3.Callsign = "T300";
            testUnit3.UnitType = "Test Unit 3";
            testUnit3.FromStatus = "";
            testUnit3.UnitLocation = "";
            testUnit3.UnitHomeStation = "";
            testUnit3.UnitCurrentStation = "";

            CodingUnits testUnit4 = new CodingUnits();
            testUnit4.ID = 456;
            testUnit4.Callsign = "T400";
            testUnit4.UnitType = "Test Unit 4";
            testUnit4.FromStatus = "";
            testUnit4.UnitLocation = "";
            testUnit4.UnitHomeStation = "";
            testUnit4.UnitCurrentStation = "";

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

        public void InitStationDataGrid()
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

        public void GatewayRcvConnStatus(string station)
        {
            SendOrPostCallback callback =
                delegate(object state)
                {
                    _ToBeRemove.Remove(station);
                };

            _uiSyncContext.Post(callback, "rcv conn status reply");
        }

        public void EditConnStatus(string StationName, string Status)
        {
            SendOrPostCallback callback =
                delegate(object state)
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
        public void NotifyConsoleNotConnected(string station, CodingIncidentMessage codingIncidentMsg)
        {
            SendOrPostCallback callback =
                delegate(object state)
                {
                    //Updated coding status failed
                    UpdateCodingStatus(codingIncidentMsg.CodingID, "Failed");

                    //Updated message status failed
                    MessageStatus messagestatus = UpdateMessageStatus(codingIncidentMsg.CodingID, station, "Failed");

                    List<string> unitcallsign = new List<string>();
                    //To give relevant station units callsign
                    foreach (CodingUnits unit in codingIncidentMsg.DispatchUnits)
                    {
                        if (unit.UnitCurrentStation.Equals(station))
                        {
                            unitcallsign.Add(unit.Callsign);
                        }
                    }

                    //Broadcast Coding status back to CAD (failed)
                    SendBroadcastIncidentCoding(station, "Failed", unitcallsign.ToArray(), messagestatus);
                };

            _uiSyncContext.Post(callback, "Notify deliever failed");
        }

        public void RcvCodingAckMsg(CodingAckMessage codingAckMsg)
        {
            SendOrPostCallback callback =
                delegate(object state)
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

                };

            _uiSyncContext.Post(callback, "updated Coding Ack Msg");

        }

        public void SendBroadcastIncidentCoding(string station, string status, string[] unitcallsign, MessageStatus messagestatus)
        {
            //Get Station tracking
            Tracking stationTrack = new Tracking();
            stationTrack.Station = station;
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
            _CallOut_CADService.BroadcastIncidentCodingStatus(incidentcodingstatus);
        }

        public void StartTargetTimeoutTimer(string console, CodingIncidentMessage codingIncidentMsg)
        {
            SendOrPostCallback callback =
                delegate(object state)
                {
                    Debug.WriteLine("StartTargetTimeoutTimer");
                    //Start timer for 15 sec to assume console disconnected
                    System.Timers.Timer AutoFailedTimer = new System.Timers.Timer();
                    AutoFailedTimer.Interval = 15000; //15 sec
                    AutoFailedTimer.Elapsed += delegate { TargetTimeout(console, codingIncidentMsg.CodingID, codingIncidentMsg.DispatchUnits); };
                    AutoFailedTimer.AutoReset = false;
                    AutoFailedTimer.Start();
                };

            _uiSyncContext.Post(callback, "start target timeout timer");
        }

        public void TargetTimeout(string console, string codingID, CodingUnits[] dispatchUnits)
        {
            Debug.WriteLine("TargetTimeout");
            //Cross thread 
            SendOrPostCallback callback = delegate(object state)
            {
                //string codingID = codingIncidentMsg.CodingID;

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

                            //Notify those failed msg to console
                            //_CallOut_CodingService.RemovefromMsgQueue(console, codingID);

                            if (nomatchinmsgstatus)
                            {
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
                                    if (unit.UnitCurrentStation.Equals(""))
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
        public void RcvCADIncidentMsg(DispatchedIncident CADincidentMsg)
        {
            SendOrPostCallback callback =
                delegate(object state)
                {
                    List<string> tmpstationList = new List<string>();
                    List<Tracking> trackingList = new List<Tracking>();
                    //Only take out the stations on the Current Station
                    foreach (DispatchedUnit uniqueunit in CADincidentMsg.ListOfUnits)
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
                            foreach (DispatchedUnit unit in CADincidentMsg.ListOfUnits)
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
                    ConvertCodingtoTracker(codingincidentmsg); //Add to gateway tracker
                    _CallOut_CodingService.TargetMsg(addressList, codingincidentmsg);

                    //Send ack back to CAD
                    CADIncidentAck cadincidentack = new CADIncidentAck();
                    cadincidentack.CodingID = codingincidentmsg.CodingID;
                    cadincidentack.AckTracking = trackingList.ToArray();
                    DateTime currentdt = DateTime.Now;
                    cadincidentack.AckTimeStamp = currentdt;
                    cadincidentack.AckNo = 0;
                    cadincidentack.AckTotal = tmpstationList.Count;

                    _CallOut_CADService.AckCADIncidentMsg(cadincidentack);


                    //Set Coding Entry
                    CreateCodingEntry(codingincidentmsg, tmpstationList.Count.ToString());
                    //Set Message Entry
                    CreateMessageEntry(codingincidentmsg, tmpstationList.Count.ToString());

                };

            _uiSyncContext.Post(callback, "Rcv Incident Message");
        }

        public CodingIncidentMessage ConvertIncidentToCoding(DispatchedIncident CADincidentMsg)
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
            foreach (DispatchedUnit unit in CADincidentMsg.ListOfUnits)
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
        public void ConvertCodingtoTracker(CodingIncidentMessage CodingincidentMsg)
        {
            GatewayTracker gatewaytrack = new GatewayTracker();
            gatewaytrack.CodingID = CodingincidentMsg.CodingID;
            gatewaytrack.IncidentID = CodingincidentMsg.IncidentNo;

            List<CodingUnits> dispatchunits = new List<CodingUnits>();
            Dictionary<string, string> stationstatus = new Dictionary<string, string>();
            foreach (CodingUnits unit in CodingincidentMsg.DispatchUnits)
            {
                dispatchunits.Add(unit);

                if (!stationstatus.ContainsKey(unit.UnitCurrentStation))
                {
                    stationstatus.Add(unit.UnitCurrentStation, "Pending");
                }
            }
            gatewaytrack.DispatchUnits = dispatchunits;
            gatewaytrack.StationStatus = stationstatus;

            _GatewayTrackerList.Add(gatewaytrack);
        }
        
        //Retrieve Incident Coding Status base on Query
        public void IncidentCodingStatus(string querycodingID)
        {
            SendOrPostCallback callback =
                delegate(object state)
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

                        List<string> tmpstationList = new List<string>();
                        List<Tracking> trackingList = new List<Tracking>();

                        foreach (GatewayTracker gatewaytrack in _GatewayTrackerList)
                        {
                            if (gatewaytrack.CodingID.Equals(querycodingID))
                            {
                                //Only take out the stations on the Current Station
                                foreach (CodingUnits uniqueunit in gatewaytrack.DispatchUnits)
                                {
                                    //Avoid duplicate station name in the list
                                    if (!tmpstationList.Contains(uniqueunit.UnitCurrentStation))
                                    {
                                        tmpstationList.Add(uniqueunit.UnitCurrentStation);
                                        Tracking newstation = new Tracking();
                                        newstation.Station = uniqueunit.UnitCurrentStation;

                                        //get the status from message status
                                        foreach (MessageStatus msgstatus in _MessageStatusList)
                                        {

                                            //If exist in message status ack/reject/failed
                                            if (msgstatus.CodingID.Equals(querycodingID) && msgstatus.AckFrom.Equals(uniqueunit.UnitCurrentStation))
                                            {
                                                newstation.Status = msgstatus.AckStatus;
                                                break;
                                            }
                                            //else its still pending
                                            newstation.Status = "Pending";
                                        }

                                        List<string> unitcallsign = new List<string>();

                                        //To give relevant station units callsign
                                        foreach (CodingUnits unit in gatewaytrack.DispatchUnits)
                                        {
                                            if (unit.UnitCurrentStation.Equals(uniqueunit.UnitCurrentStation))
                                            {
                                                unitcallsign.Add(unit.Callsign);
                                            }
                                        }

                                        newstation.Unit = unitcallsign.ToArray();
                                        trackingList.Add(newstation); //Add into tracking list
                                    }
                                }
                            }
                        }

                        CADIncidentAck codingQueryRsponse = new CADIncidentAck();
                        codingQueryRsponse.CodingID = newMsgStatus.CodingID;
                        codingQueryRsponse.AckTracking = trackingList.ToArray();
                        codingQueryRsponse.AckTimeStamp = currentdt;
                        codingQueryRsponse.AckNo = Int32.Parse(newMsgStatus.AckNo);
                        codingQueryRsponse.AckTotal = Int32.Parse(newMsgStatus.AckTotal);
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
