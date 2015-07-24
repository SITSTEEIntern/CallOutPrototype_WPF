using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gCAD.Shared.IntegrationContract;
using System.ServiceModel;

using CallOut_Gateway;

namespace CalloutServices
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class DispatchService : IDispatchIntegrationNotification
    {
        public DispatchService()
        { }

        public void IncidentDispatched(DispatchedIncident incident)
        {
            //create instance of form1 to access to the method needed in form1
            Form1.Instance.RcvDispatchedIncidentMsg(incident);
        }
    }

}
