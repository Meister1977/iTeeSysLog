using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iTeeSysLog
{
    public static class SysLogListener
    {
        private static readonly log4net.ILog Log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static bool StopRequested;
        private static Thread _listenerThread;
        private static IPEndPoint _anyIP;
        private static UdpClient _udpListener;



        public static void SetUp()
        {
            _anyIP = new IPEndPoint(IPAddress.Any, 0);
            _udpListener = new UdpClient(514);
            _listenerThread = new Thread(Listener);
            _listenerThread.Start();
        }

        private static void Listener()
        {
            while (!StopRequested)
            {
                var bReceive = _udpListener.Receive(ref _anyIP);
                var sReceive = Encoding.ASCII.GetString(bReceive);
                Task.Run(()=> WriteLog(sReceive));
            }
            _udpListener.Close();
        }

        private static void WriteLog(string sReceive)
        {
            if (sReceive.Contains("miniupnpd")
                || sReceive.Contains("hostapd: wlan1: STA")
                || sReceive.Contains("dnsmasq-dhcp")
                || sReceive.Contains("hostapd: wlan0"))
                return;

            Log.Info(sReceive);
        }

        public static void Stop()
        {
            StopRequested = true;
            _listenerThread.Interrupt();
            if (!_listenerThread.Join(300))
            { 
                _listenerThread.Abort();
            }
            _udpListener.Close();
        }
    }
}