using Machina;
using Machina.FFXIV;
using System;
using System.Collections.Generic;

namespace FFXIVOpcodeWizard
{
    public enum Region : byte
    {
        Global,
        KR,
        CN
    }

    class FFXIVOpcodeWizard
    {
        static LinkedList<Packet> pq;

        static bool readYes()
        {
            return Console.ReadLine().ToLower().StartsWith("y");
        }

        static void Main(string[] args)
        {
            // Packet queue
            pq = new LinkedList<Packet>();

            // Get game region
            Console.WriteLine("Are you using the Chinese game client? [y/N]");
            Region region = readYes() ? Region.CN : Region.Global;

            Console.WriteLine("Use WinPcap instead of RawSocket? [y/N]");
            TCPNetworkMonitor.NetworkMonitorType MonitorType = readYes() 
                ? TCPNetworkMonitor.NetworkMonitorType.WinPCap
                : TCPNetworkMonitor.NetworkMonitorType.RawSocket;

            // Initialize Machina
            FFXIVNetworkMonitor monitor = new FFXIVNetworkMonitor
            {
                MessageReceived = OnMessageReceived,
                MessageSent = OnMessageSent,
                MonitorType = MonitorType,
                Region = region
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
