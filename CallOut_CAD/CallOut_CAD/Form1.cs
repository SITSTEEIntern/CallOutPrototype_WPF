using System;
//using System.Collections.Generic;
//using System.Data;
//using System.Drawing;
using System.Threading;
using System.ServiceModel;
//using System.Linq;
//using System.Text;
using System.Windows.Forms;
using System.Diagnostics; //for debug
using System.ComponentModel; //for bindinglist, inotify
using System.Timers; //Timer

// Location of the proxy.
using CallOut_CAD.ServiceReference1;

using CallOut_CAD.Class;

namespace CallOut_CAD
{
    // Specify for the callback to NOT use the current synchronization context
    [CallbackBehavior(
        ConcurrencyMode = ConcurrencyMode.Single,
        UseSynchronizationContext = false)]
    public partial class Form1 : Form, ServiceReference1.CallOut_CADServiceCallback
    {
        //Declaration
        private SynchronizationContext _uiSyncContext = null;
        private ServiceReference1.CallOut_CADServiceClient _CallOut_CADService = null;

        //For toggle of button
        private bool _isCADConnected = false;

        //BindingList to Data Grid View
        BindingList<IncidentCodingStatus> _IncidentCodingStatusList = new BindingList<IncidentCodingStatus>();

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
            _CallOut_CADService = new ServiceReference1.CallOut_CADServiceClient(new InstanceContext(this), "WSDualHttpBinding_CallOut_CADService");
            _CallOut_CADService.Open();

            // Initial eventhandlers
            this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);

            //Init datagridview
            InitIncidentCodingStatusDataGrid();

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //Terminate the connection to the service.
            if (_isCADConnected)
            {
                _CallOut_CADService.Close();
            }

        }

        private void btnJoinCAD_Click(object sender, EventArgs e)
        {
            if (_isCADConnected)
            {
                // Let the service know that this user is leaving
                _CallOut_CADService.CADLeave();

                //Toggle button display
                _isCADConnected = false;
                this.btnJoinCAD.Text = "Join CAD";
            }
            else
            {
                //contact the service.
                _CallOut_CADService.CADJoin();

                //Toggle button display
                _isCADConnected = true;
                this.btnJoinCAD.Text = "Leave CAD";
            }
        }

        //Test one location
        private void btnSample1_Click(object sender, EventArgs e)
        {
            string incidentNo = "ICD-001";
            string incidentTitle = "Fire in AMK Hub";

            Address incidentlocation = new Address();
            incidentlocation.Name = "AMK Hub";
            incidentlocation.Street = "1 Ang Mo Kio Electronics Park Road";
            incidentlocation.Unit = "1234";
            incidentlocation.State = "SG";
            incidentlocation.City = "SG";
            incidentlocation.Country = "SG";
            incidentlocation.PostalCode = "123456";

            string incidentType = "SMALL FIRE";
            int incidentAlarm = 1;
            string incidentPriority = "1";
            DateTime dispatchDateTime = DateTime.Now;

            DispatchedUnit dispatchUnit1 = new DispatchedUnit();
            dispatchUnit1.ID = 123;
            dispatchUnit1.CallSign = "A111";
            dispatchUnit1.UnitType = "Ambulance";
            dispatchUnit1.Status = "AB";
            dispatchUnit1.Location = "Ang Mo Kio FS";
            dispatchUnit1.HomeStation = "Alexandra FS";
            dispatchUnit1.CurrentStation = "Ang Mo Kio FS";

            DispatchedUnit[] dispatchUnits = new DispatchedUnit[1];
            dispatchUnits[0] = dispatchUnit1;

            DispatchedIncident SampleMsg1 = new DispatchedIncident();
            SampleMsg1.IncidentNumber = incidentNo;
            SampleMsg1.IncidentTitle = incidentTitle;
            SampleMsg1.IncidentLocation = incidentlocation;
            SampleMsg1.IncidentType = incidentType;
            SampleMsg1.AlarmLevel = incidentAlarm;
            SampleMsg1.Priority = incidentPriority;
            SampleMsg1.DateTime = dispatchDateTime;
            SampleMsg1.ListOfUnits = dispatchUnits;

            _CallOut_CADService.SendCADIncidentMsg(SampleMsg1);
        }

        //Test Mutiple Location
        private void btnSample2_Click(object sender, EventArgs e)
        {
            string incidentNo = "ICD-002";
            string incidentTitle = "Fire in AMK Hub";

            Address incidentlocation = new Address();
            incidentlocation.Name = "AMK Hub";
            incidentlocation.Street = "1 Ang Mo Kio Electronics Park Road";
            incidentlocation.Unit = "1234";
            incidentlocation.State = "SG";
            incidentlocation.City = "SG";
            incidentlocation.Country = "SG";
            incidentlocation.PostalCode = "123456";

            string incidentType = "SMALL FIRE";
            int incidentAlarm = 1;
            string incidentPriority = "1";
            DateTime dispatchDateTime = DateTime.Now;

            DispatchedUnit dispatchUnit1 = new DispatchedUnit();
            dispatchUnit1.ID = 123;
            dispatchUnit1.CallSign = "A111";
            dispatchUnit1.UnitType = "Ambulance";
            dispatchUnit1.Status = "AB";
            dispatchUnit1.Location = "Ang Mo Kio FS";
            dispatchUnit1.HomeStation = "Alexandra FS";
            dispatchUnit1.CurrentStation = "Ang Mo Kio FS";

            DispatchedUnit dispatchUnit2 = new DispatchedUnit();
            dispatchUnit1.ID = 789;
            dispatchUnit2.CallSign = "A222";
            dispatchUnit2.UnitType = "Ambulance";
            dispatchUnit2.Status = "AB";
            dispatchUnit2.Location = "Ang Mo Kio FS";
            dispatchUnit2.HomeStation = "Alexandra FS";
            dispatchUnit2.CurrentStation = "Central FS";

            DispatchedUnit[] dispatchUnits = new DispatchedUnit[2];
            dispatchUnits[0] = dispatchUnit1;
            dispatchUnits[1] = dispatchUnit2;

            DispatchedIncident SampleMsg2 = new DispatchedIncident();
            SampleMsg2.IncidentNumber = incidentNo;
            SampleMsg2.IncidentTitle = incidentTitle;
            SampleMsg2.IncidentLocation = incidentlocation;
            SampleMsg2.IncidentType = incidentType;
            SampleMsg2.AlarmLevel = incidentAlarm;
            SampleMsg2.Priority = incidentPriority;
            SampleMsg2.DateTime = dispatchDateTime;
            SampleMsg2.ListOfUnits = dispatchUnits;

            _CallOut_CADService.SendCADIncidentMsg(SampleMsg2);
        }

        //Test Mutiple and have duplicate Location
        private void btnSample3_Click(object sender, EventArgs e)
        {
            string incidentNo = "ICD-003";
            string incidentTitle = "Fire in AMK Hub";

            Address incidentlocation = new Address();
            incidentlocation.Name = "AMK Hub";
            incidentlocation.Street = "1 Ang Mo Kio Electronics Park Road";
            incidentlocation.Unit = "1234";
            incidentlocation.State = "SG";
            incidentlocation.City = "SG";
            incidentlocation.Country = "SG";
            incidentlocation.PostalCode = "123456";

            string incidentType = "SMALL FIRE";
            int incidentAlarm = 1;
            string incidentPriority = "1";
            DateTime dispatchDateTime = DateTime.Now;

            DispatchedUnit dispatchUnit1 = new DispatchedUnit();
            dispatchUnit1.ID = 123;
            dispatchUnit1.CallSign = "A111";
            dispatchUnit1.UnitType = "Ambulance";
            dispatchUnit1.Status = "AB";
            dispatchUnit1.Location = "Ang Mo Kio FS";
            dispatchUnit1.HomeStation = "Alexandra FS";
            dispatchUnit1.CurrentStation = "Ang Mo Kio FS";

            DispatchedUnit dispatchUnit2 = new DispatchedUnit();
            dispatchUnit2.ID = 123;
            dispatchUnit2.CallSign = "A112";
            dispatchUnit2.UnitType = "Ambulance";
            dispatchUnit2.Status = "AB";
            dispatchUnit2.Location = "Ang Mo Kio FS";
            dispatchUnit2.HomeStation = "Alexandra FS";
            dispatchUnit2.CurrentStation = "Ang Mo Kio FS";

            DispatchedUnit[] dispatchUnits = new DispatchedUnit[2];
            dispatchUnits[0] = dispatchUnit1;
            dispatchUnits[1] = dispatchUnit2;

            DispatchedIncident SampleMsg3 = new DispatchedIncident();
            SampleMsg3.IncidentNumber = incidentNo;
            SampleMsg3.IncidentTitle = incidentTitle;
            SampleMsg3.IncidentLocation = incidentlocation;
            SampleMsg3.IncidentType = incidentType;
            SampleMsg3.AlarmLevel = incidentAlarm;
            SampleMsg3.Priority = incidentPriority;
            SampleMsg3.DateTime = dispatchDateTime;
            SampleMsg3.ListOfUnits = dispatchUnits;

            _CallOut_CADService.SendCADIncidentMsg(SampleMsg3);
        }

        private void btnQuery_Click(object sender, EventArgs e)
        {
            _CallOut_CADService.IncidentCodingStatusQuery(this.txtQuery.Text);
        }

        #region Initialise CAD Incident Coding Status Data Grid View

        public void InitIncidentCodingStatusDataGrid()
        {
            //Initialize the datagridview
            this.dgvIncident.AutoGenerateColumns = false;
            this.dgvIncident.CellBorderStyle = DataGridViewCellBorderStyle.None;

            DataGridViewTextBoxColumn codingIDColumn = new DataGridViewTextBoxColumn();
            codingIDColumn.DataPropertyName = "CodingID";
            codingIDColumn.HeaderText = "CodingID";
            codingIDColumn.ReadOnly = true;
            codingIDColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

            DataGridViewTextBoxColumn trackstationColumn = new DataGridViewTextBoxColumn();
            trackstationColumn.DataPropertyName = "TrackStation";
            trackstationColumn.HeaderText = "TrackStation";
            trackstationColumn.ReadOnly = true;
            trackstationColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

            DataGridViewTextBoxColumn trackstatusColumn = new DataGridViewTextBoxColumn();
            trackstatusColumn.DataPropertyName = "TrackStatus";
            trackstatusColumn.HeaderText = "TrackStatus";
            trackstatusColumn.ReadOnly = true;
            trackstatusColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

            DataGridViewTextBoxColumn trackunitColumn = new DataGridViewTextBoxColumn();
            trackunitColumn.DataPropertyName = "TrackUnit";
            trackunitColumn.HeaderText = "TrackUnit";
            trackunitColumn.ReadOnly = true;
            trackunitColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

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

            DataGridViewTextBoxColumn acktimeColumn = new DataGridViewTextBoxColumn();
            acktimeColumn.DataPropertyName = "AckTimeStamp";
            acktimeColumn.HeaderText = "ackTimeStamp";
            acktimeColumn.ReadOnly = true;
            acktimeColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

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

            this.dgvIncident.Columns.Add(codingIDColumn);
            this.dgvIncident.Columns.Add(trackstationColumn);
            this.dgvIncident.Columns.Add(trackstatusColumn);
            this.dgvIncident.Columns.Add(trackunitColumn);
            this.dgvIncident.Columns.Add(ackfromColumn);
            this.dgvIncident.Columns.Add(ackstatusColumn);
            this.dgvIncident.Columns.Add(acktimeColumn);
            this.dgvIncident.Columns.Add(ackNoColumn);
            this.dgvIncident.Columns.Add(acktotalColumn);

            _IncidentCodingStatusList.Clear();

            this.dgvIncident.DataSource = _IncidentCodingStatusList;
        }

        #endregion

        #region CallOut_CADServiceCallback Methods

        //Receive Ack from gateway and update according at the CAD
        public void UpdateCADIncidentAck(CADIncidentAck CADincidentack)
        {
            SendOrPostCallback callback =
                delegate(object state)
                {
                    //for each station in the tracking
                    foreach(Tracking station in CADincidentack.AckTracking){

                        //for each unit in the station
                        foreach (string unitcallsign in station.Unit)
                        {
                            IncidentCodingStatus newincidentcodingsttaus = new IncidentCodingStatus();
                            newincidentcodingsttaus.CodingID = CADincidentack.CodingID;
                            newincidentcodingsttaus.TrackStation = station.Station;
                            newincidentcodingsttaus.TrackStatus = station.Status;
                            newincidentcodingsttaus.TrackUnit = unitcallsign;
                            newincidentcodingsttaus.AckFrom = "";
                            newincidentcodingsttaus.AckStatus = "";
                            newincidentcodingsttaus.AckTimeStamp = String.Format("{0:g}", CADincidentack.AckTimeStamp);
                            newincidentcodingsttaus.AckNo = CADincidentack.AckNo.ToString();
                            newincidentcodingsttaus.AckTotal = CADincidentack.AckTotal.ToString();

                            _IncidentCodingStatusList.Add(newincidentcodingsttaus);
                        }
                    }
                };

            _uiSyncContext.Post(callback, "update CAD Incident Ack");
        }

        //keep update once there is a respond from the console to gateway
        public void UpdateIncidentCodingStatus(CADIncidentCodingStatus incidentcodingstatus)
        {
            SendOrPostCallback callback =
                delegate(object state)
                {
                    foreach (string unitcallsign in incidentcodingstatus.AckTracking.Unit)
                    {
                        IncidentCodingStatus newincidentcodingsttaus = new IncidentCodingStatus();
                        newincidentcodingsttaus.CodingID = incidentcodingstatus.CodingID;
                        newincidentcodingsttaus.TrackStation = incidentcodingstatus.AckTracking.Station;
                        newincidentcodingsttaus.TrackStatus = incidentcodingstatus.AckTracking.Status;
                        newincidentcodingsttaus.TrackUnit = unitcallsign;
                        newincidentcodingsttaus.AckFrom = incidentcodingstatus.AckFrom;
                        newincidentcodingsttaus.AckStatus = incidentcodingstatus.AckStatus;
                        newincidentcodingsttaus.AckTimeStamp = String.Format("{0:g}", incidentcodingstatus.AckTimeStamp);
                        newincidentcodingsttaus.AckNo = incidentcodingstatus.AckNo.ToString();
                        newincidentcodingsttaus.AckTotal = incidentcodingstatus.AckTotal.ToString();

                        _IncidentCodingStatusList.Add(newincidentcodingsttaus);
                    }

                };

            _uiSyncContext.Post(callback, "update Incident Coding Status");
        }

        //Update according at the CAD from thr adhoc Incident Coding Query
        public void RcvIncidentCodingStatusResponse(CADIncidentAck codingstatusresponse)
        {
            SendOrPostCallback callback =
                delegate(object state)
                {
                    //for each station in the tracking
                    foreach (Tracking station in codingstatusresponse.AckTracking)
                    {
                        //for each unit in the station
                        foreach (string unitcallsign in station.Unit)
                        {
                            IncidentCodingStatus newincidentcodingsttaus = new IncidentCodingStatus();
                            newincidentcodingsttaus.CodingID = codingstatusresponse.CodingID;
                            newincidentcodingsttaus.TrackStation = station.Station;
                            newincidentcodingsttaus.TrackStatus = station.Status;
                            newincidentcodingsttaus.TrackUnit = unitcallsign;
                            newincidentcodingsttaus.AckFrom = "";
                            newincidentcodingsttaus.AckStatus = "";
                            newincidentcodingsttaus.AckTimeStamp = String.Format("{0:g}", codingstatusresponse.AckTimeStamp);
                            newincidentcodingsttaus.AckNo = codingstatusresponse.AckNo.ToString();
                            newincidentcodingsttaus.AckTotal = codingstatusresponse.AckTotal.ToString();

                            _IncidentCodingStatusList.Add(newincidentcodingsttaus);
                        }
                    }
                };

            _uiSyncContext.Post(callback, "update incident coding status responese");
        }

        #region Method not for CAD
        public void RcvCADIncidentMsg(DispatchedIncident CADincidentMsg)
        { }
        public void IncidentCodingStatus(string querycodingID)
        { }
        #endregion


        #endregion


    }
}
