//using System;
using System.Collections.Generic;
//using System.Linq;
//using System.Text;
using System.IO; //for read from file
using System.Diagnostics; //for debug
using System.ServiceModel;// for WCF to happen
using System.Runtime.Serialization;//datacontract

using CallOut_CodingServiceLib.Class;

namespace CallOut_CodingServiceLib
{
    /// <summary>
    /// Call-Out Service Library
    /// </summary>
    [ServiceContract(
    Name = "CallOut_CodingService",
    Namespace = "CallOut_CodingServiceLib",
    SessionMode = SessionMode.Required,
    CallbackContract = typeof(IMessageServiceCallback))]

    public interface IMessageServiceInbound
    {
        //Pass Station Name to Gateway and Console
        [OperationContract]
        List<StationStatus> GetStationStatus();
        [OperationContract(IsOneWay = true)]
        void UpdateStationStatus(string status, string station, string timestamp);
        [OperationContract]
        List<string> GetConnectedConsole();

        //For CodeID and TestID
        [OperationContract]
        string GetCodingID();
        [OperationContract]
        string GetTestID();
        [OperationContract]
        string GetConnectedConsoleNo();

        //Gateway Join in the service
        [OperationContract]
        void GatewayJoin();
        [OperationContract]
        void GatewayLeave();

        //Allow Console add and remove its channel in the service
        [OperationContract]
        void ConsoleJoin(string userName);
        [OperationContract]
        void ConsoleLeave(string userName);

        //*Coding Incident Message*
        //Send test message from Gateway to Console 
        //(in a form of object same as how Gateway pass CAD message to Console)
        [OperationContract(IsOneWay = true)]
        void TargetMsg(List<string> addressList, CodingIncidentMessage codingIncidentMsg);

        //*Coding Ack Message*
        //Reply coding ack msg back to gateway
        [OperationContract(IsOneWay = true)]
        void AckCodingIncidentMsg(CodingAckMessage codingAckMsg);

        //*Health Check*
        //Request for conn status
        [OperationContract(IsOneWay = true)]
        void RequestConnStatus();
        //Response to health check request
        //No reply means offline or lag?
        [OperationContract(IsOneWay = true)]
        void ReplyConnStatus(string station);

        //Once the msg in gateway had reached timeout, it will inform console and remove msg from queue
        //[OperationContract(IsOneWay = true)]
        //void RemovefromMsgQueue(string station, string CodingID);

        //Once Message is displayed on console screen, it will send a response to gateway to start timeout timer [console]
        [OperationContract(IsOneWay = true)]
        void MsgDisplayedResponse(string console, CodingIncidentMessage codingIncidentMsg);
    }

    public interface IMessageServiceCallback
    {
        //[Console]
        [OperationContract(IsOneWay = true)]
        void ConsoleDisplayMsg(CodingIncidentMessage codingIncidentMsg);

        //Callback to trigger changes to Connection Status List [Gateway]
        [OperationContract(IsOneWay = true)]
        void EditConnStatus(string Name, string Status);

        ////Send Coding Message failed due to console not connected [Gateway]
        [OperationContract(IsOneWay = true)]
        void NotifyConsoleNotConnected(string station, CodingIncidentMessage codingIncidentMsg);

        //Receive Coding Ack Message from Console and update [Gateway]
        [OperationContract(IsOneWay = true)]
        void RcvCodingAckMsg(CodingAckMessage codingAckMsg);

        //Receive the conn status for healthcheck
        [OperationContract(IsOneWay = true)]
        void GatewayRcvConnStatus(string station);
        [OperationContract(IsOneWay = true)]
        void ConsoleRcvConnStatus();

        //[Console] received the station that had already failed/timeout at gateway
        //[OperationContract(IsOneWay = true)]
        //void UpdateRemoveMsgList(string CodingID);

        //[Gateway] Start timer for timeout of the incident that displayed on console screen
        [OperationContract(IsOneWay = true)]
        void StartTargetTimeoutTimer(string console, CodingIncidentMessage codingIncidentMsg);
    }

    /// <summary>
    /// Call Out Service behaviour
    /// </summary>
    /// <remarks>
    /// Single is the default concurrency mode, but it never hurts to be explicit.
    /// Note the single threading model still functions properly with use of one way methods for callbacks 
    /// since they are marked as one way on the contract.
    /// </remarks>
    [ServiceBehavior(
            ConcurrencyMode = ConcurrencyMode.Single,
            InstanceContextMode = InstanceContextMode.PerCall)]

    public class CallOut_CodingService : IMessageServiceInbound
    {
        //List of callback channel in order find the link back
        private static List<IMessageServiceCallback> _ConsoleCallbackList = new List<IMessageServiceCallback>();
        private static List<IMessageServiceCallback> _GatewayCallbackList = new List<IMessageServiceCallback>();

        private static List<string> _ConnectedConsoleList = new List<string>();
        private static Dictionary<string, IMessageServiceCallback> _ConnectedConsoleDict =
        new Dictionary<string, IMessageServiceCallback>();// Default Constructor

        //List for connection status 
        private static List<StationStatus> StationIDList = new List<StationStatus>();

        //Number of connected console
        private static int _ConnectedConsoleNo = 0;

        //Keep track of CodingID and TestID in service to control
        private static int _CodingNo = 1;
        private static int _TestNo = 1;

        // Default Constructor
        public CallOut_CodingService()
        {

            //read from file to store the station name (only run one time for the first server)
            if (StationIDList.Count == 0)
            {
                string[] lineOfContents = File.ReadAllLines(@"configFile.txt");
                foreach (var name in lineOfContents)
                {
                    StationIDList.Add(new StationStatus("Offline", name, ""));
                }
            }
        }

        /*
         * Return the List of station, status, last updated time
         */
        public List<StationStatus> GetStationStatus()
        {
            return StationIDList;
        }

        /*
         * Update the Station status list when there is changes, 
         * in order to have updated information when a new gateway is created.
         */
        public void UpdateStationStatus(string status, string station, string timestamp)
        {
            foreach (StationStatus stationID in StationIDList)
            {
                if (stationID.Station.Equals(station))
                {
                    stationID.Status = status;
                    stationID.Update = timestamp;
                }
            }
        }

        public List<string> GetConnectedConsole()
        {
            return _ConnectedConsoleList;
        }

        /*
         * Return the number of connected console
         */
        public string GetConnectedConsoleNo()
        {
            return _ConnectedConsoleNo.ToString();
        }

        /*
         * Return the CodingID
         */
        public string GetCodingID()
        {
            string tmpcodeid = _CodingNo.ToString();
            _CodingNo++;
            return "COD-" + tmpcodeid;
        }

        /*
         * Return the TestID
         */
        public string GetTestID()
        {
            string tmptestid = _TestNo.ToString();
            _TestNo++;
            return "TEST-" + tmptestid;
        }

        /*
         * Server join and leave in order to place a channel path here
         */
        public void GatewayJoin()
        {
            // Subscribe the user to the conversation
            IMessageServiceCallback connectedGateway = OperationContext.Current.GetCallbackChannel<IMessageServiceCallback>();

            if (!_GatewayCallbackList.Contains(connectedGateway))
            {
                _GatewayCallbackList.Add(connectedGateway);//Note the callback list is just a list of channels.
            }
        }

        public void GatewayLeave()
        {
            //Unsubscribe the user to the conversation
            IMessageServiceCallback connectedGateway = OperationContext.Current.GetCallbackChannel<IMessageServiceCallback>();

            if (_GatewayCallbackList.Contains(connectedGateway))
            {
                _GatewayCallbackList.Remove(connectedGateway);//Note the callback list is just a list of channels.

            }
        }

        //Sub to server to receive msg from server
        public void ConsoleJoin(string console)
        {
            // Add the connected console channel into the list
            IMessageServiceCallback connectedConsole = OperationContext.Current.GetCallbackChannel<IMessageServiceCallback>();

            if (!_ConsoleCallbackList.Contains(connectedConsole))
            {
                _ConsoleCallbackList.Add(connectedConsole);//Note the callback list is just a list of channels.
                _ConnectedConsoleList.Add(console);
                _ConnectedConsoleDict.Add(console, connectedConsole);//Bind the username to the callback channel ID
                _ConnectedConsoleNo++;

                //Update the station status (for case if mutiple gateway existed)
                foreach (StationStatus station in StationIDList)
                {
                    if (station.Station.Equals(console))
                    {
                        station.Status = "Online";
                    }
                }

                //Handle case where 2 client log in as same ID
            }

            _GatewayCallbackList.ForEach(
                delegate(IMessageServiceCallback gatewaycallback)
                {
                    gatewaycallback.EditConnStatus(console, "Online");
                });

        }

        public void ConsoleLeave(string console)
        {
            // Unsubscribe the user from the conversation. 
            //if station is in the dictionary list (to avoid calling consoleLeave twice)
            if (_ConnectedConsoleDict.ContainsKey(console))
            {
                IMessageServiceCallback connectedConsole = _ConnectedConsoleDict[console];

                if (_ConsoleCallbackList.Contains(connectedConsole))
                {
                    _ConsoleCallbackList.Remove(connectedConsole);
                    _ConnectedConsoleDict.Remove(console);
                    _ConnectedConsoleList.Remove(console);
                    _ConnectedConsoleNo--;

                    //Update the station status
                    foreach (StationStatus station in StationIDList)
                    {
                        if (station.Station.Equals(console))
                        {
                            station.Status = "Offline";
                        }
                    }
                }

                _GatewayCallbackList.ForEach(
                    delegate(IMessageServiceCallback gatewaycallback)
                    {
                        gatewaycallback.EditConnStatus(console, "Offline");
                    });
            }
        }

        public void TargetMsg(List<string> addressList, CodingIncidentMessage codingIncidentMsg)
        {
            foreach (string station in addressList)
            {
                //If is in connected console 
                //else reply "failed" back to gateway
                //to prevent from search on an invalid key in the dictionary of connected console
                if (_ConnectedConsoleList.Contains(station))
                {
                    IMessageServiceCallback tmpCallback = _ConnectedConsoleDict[station];
                    tmpCallback.ConsoleDisplayMsg(codingIncidentMsg);
                }
                else
                {
                    _GatewayCallbackList.ForEach(
                    delegate(IMessageServiceCallback gatewaycallback)
                    {
                        gatewaycallback.NotifyConsoleNotConnected(station, codingIncidentMsg);
                    });
                }
            }
        }

        public void AckCodingIncidentMsg(CodingAckMessage codingAckMsg)
        {
            _GatewayCallbackList.ForEach(
                delegate(IMessageServiceCallback gatewaycallback)
                {
                    gatewaycallback.RcvCodingAckMsg(codingAckMsg);
                });
        }

        public void RequestConnStatus()
        {
            foreach (string station in _ConnectedConsoleList)
            {
                IMessageServiceCallback tmpCallback = _ConnectedConsoleDict[station];
                if (((ICommunicationObject)tmpCallback).State == CommunicationState.Opened)
                {
                    tmpCallback.ConsoleRcvConnStatus();
                }

            }
        }

        public void ReplyConnStatus(string station)
        {
            _GatewayCallbackList.ForEach(
                delegate(IMessageServiceCallback gatewaycallback)
                {
                    gatewaycallback.GatewayRcvConnStatus(station);
                });
        }

        //public void RemovefromMsgQueue(string station, string CodingID)
        //{
        //    if (_ConnectedConsoleList.Contains(station))
        //    {
        //        IMessageServiceCallback tmpCallback = _ConnectedConsoleDict[station];
        //        tmpCallback.UpdateRemoveMsgList(CodingID);
        //    }
        //}

        public void MsgDisplayedResponse(string console, CodingIncidentMessage codingIncidentMsg)
        {
            _GatewayCallbackList.ForEach(
                delegate(IMessageServiceCallback gatewaycallback)
                {
                    gatewaycallback.StartTargetTimeoutTimer(console, codingIncidentMsg);
                });
        }
    }
}