using Machina;
using Machina.FFXIV;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace FFXIVOpcodeWizard
{
    class FFXIVOpcodeWizard
    {
        static LinkedList<Packet> pq;

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
            pq = new LinkedList<Packet>();

            // Initialize Machina
            FFXIVNetworkMonitor monitor = new FFXIVNetworkMonitor
            {
                MessageReceived = OnMessageReceived,
                MessageSent = OnMessageSent,
                MonitorType = MonitorType
            };
            monitor.Start();

            var wizardProcessor = new WizardProcessor();

            // Run packet ID stuff
            wizardProcessor.Run(pq);
        }

        static void OnMessageReceived(string connection, long epoch, byte[] data)
        {
            OnMessage(connection, epoch, data, "inbound");
        }

        static void OnMessageSent(string connection, long epoch, byte[] data)
        {
            OnMessage(connection, epoch, data, "outbound");
        }

        static void OnMessage(string connection, long epoch, byte[] data, string direction)
        {
            pq.AddLast(new Packet(connection, epoch, data, direction));
        }
    }
}
