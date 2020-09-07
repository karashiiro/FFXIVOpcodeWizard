using FFXIVOpcodeWizard.Models;
using Machina;
using System;
using System.Collections.Generic;

namespace FFXIVOpcodeWizard
{
    public enum Region : byte
    {
        Global,
        KR,
        CN,
    }

    public static class FFXIVOpcodeWizard
    {
        static LinkedList<Packet> pq;

        private static bool ReadYes()
        {
            return Console.ReadLine()?.ToLower().StartsWith("y") ?? false;
        }

        public static void Main(string[] args)
        {
            // Packet queue
            pq = new LinkedList<Packet>();

            // Get game region
            Console.WriteLine("Are you using the Chinese game client? [y/N]");
            var region = ReadYes() ? Region.CN : Region.Global;

            Console.WriteLine("Use WinPCap instead of RawSocket (requires admin)? [y/N]");
            var monitorType = ReadYes() 
                ? TCPNetworkMonitor.NetworkMonitorType.WinPCap
                : TCPNetworkMonitor.NetworkMonitorType.RawSocket;

            // Initialize Machina
            var monitor = new FFXIVNetworkMonitor
            {
                MessageReceived = OnMessageReceived,
                MessageSent = OnMessageSent,
                MonitorType = monitorType,
                Region = region,
            };
            monitor.Start();

            var scannerRegistry = new ScannerRegistry();

            // Run packet ID stuff
            scannerRegistry.Run(pq);
        }

        private static void OnMessageReceived(string connection, long epoch, byte[] data)
        {
            OnMessage(connection, epoch, data, PacketDirection.Server);
        }

        private static void OnMessageSent(string connection, long epoch, byte[] data)
        {
            OnMessage(connection, epoch, data, PacketDirection.Client);
        }

        private static void OnMessage(string connection, long epoch, byte[] data, PacketDirection direction)
        {
            pq.AddLast(new Packet(connection, epoch, data, direction));
        }
    }
}
