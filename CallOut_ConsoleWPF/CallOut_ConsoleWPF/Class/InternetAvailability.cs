using System;
using System.Runtime;
using System.Runtime.InteropServices;

namespace CallOut_ConsoleWPF.Class
{
    public class InternetAvailability
    {
        [DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(out int description, int reservedValue);

        public bool IsInternetAvailable()
        {
            int description;
            return InternetGetConnectedState(out description, 0);
        }
    }
}
