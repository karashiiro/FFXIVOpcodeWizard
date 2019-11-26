using Machina;
using Machina.FFXIV;
using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace FFXIVOpcodeWizard
{
    class FFXIVOpcodeWizard
    {
        [DllImport("wpcap.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr pcap_open(string source, int snaplen, int flags, int read_timeout, IntPtr auth, StringBuilder errbuff);

        static PacketQueue pq;

        static void Main(string[] args)
        {
            // Network monitor type
            TCPNetworkMonitor.NetworkMonitorType MonitorType = TCPNetworkMonitor.NetworkMonitorType.RawSocket;
            /*StringBuilder errbuff = new StringBuilder();
            pcap_open("", 0, 0, 0, new IntPtr(), errbuff);
            if (errbuff.ToString() != "")
            {
                MonitorType = TCPNetworkMonitor.NetworkMonitorType.WinPCap;
            }*/

            // Packet queue
            pq = new PacketQueue();

            // Initialize Machina
            FFXIVNetworkMonitor monitor = new FFXIVNetworkMonitor
            {
                MessageReceived = OnMessageRecieved,
                MessageSent = OnMessageSent,
                MonitorType = MonitorType
            };
            monitor.Start();

            // Run packet ID stuff
            Wizard.Run(pq);
        }

        static void OnMessageRecieved(string connection, long epoch, byte[] data)
        {
            OnMessage(connection, epoch, data, "inbound");
        }

        static void OnMessageSent(string connection, long epoch, byte[] data)
        {
            OnMessage(connection, epoch, data, "outbound");
        }

        static void OnMessage(string connection, long epoch, byte[] data, string direction)
        {
            pq.Push(new Packet(connection, epoch, data, direction));
        }
    }
}
