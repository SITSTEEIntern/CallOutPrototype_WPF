using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ServiceModel;
using CallOut_CADServiceLib;

namespace CallOut_CADServiceHost
{
    class Program
    {
        static void Main(string[] args)
        {
            using (ServiceHost serviceHost = new ServiceHost(typeof(CallOut_CADService)))
            {
                // Open the host and start listening for incoming messages.
                serviceHost.Open();
                // Keep the service running until the Enter key is pressed.
                Console.WriteLine("The CAD service is ready.");
                Console.WriteLine("Press the Enter key to terminate service.");
                Console.ReadLine();
            }
        }
    }
}
