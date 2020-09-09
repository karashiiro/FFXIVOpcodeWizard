using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FFXIVOpcodeWizard.Models;
using static Machina.TCPNetworkMonitor;

namespace FFXIVOpcodeWizard.PacketDetection
{
    public class DetectionProgram
    {
        public class Args
        {
            public ScannerRegistry Registry { get; set; }

            public Region Region { get; set; }
            public NetworkMonitorType CaptureMode { get; set; }
        }

        public class State
        {
            public int ScannerIndex { get; set; }
            public string CurrentTutorial { get; set; }
        }

        private LinkedList<Packet> pq;
        private PacketScanner scannerHost;
        private bool stopped;
        private bool skipped;

        /// <summary>
        /// Runs the detection program.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="onStateChanged">A callback function called each time a scan completes.</param>
        /// <param name="requestParameter">A function called when a parameter needs to be requested from the user.</param>
        /// <returns></returns>
        public async Task Run(Args args, Action<State> onStateChanged, Func<Scanner, int, (string parameter, bool skipRequested)> requestParameter)
        {
            this.stopped = false;
            this.pq = new LinkedList<Packet>();
            this.scannerHost = new PacketScanner();

            var state = new State();

            var monitor = new FFXIVNetworkMonitor
            {
                MessageReceived = OnMessageReceived,
                MessageSent = OnMessageSent,
                MonitorType = args.CaptureMode,
                Region = args.Region,
            };
            monitor.Start();

            var scanners = args.Registry.AsList();

            for (; state.ScannerIndex < scanners.Count; state.ScannerIndex++)
            {
                var scanner = scanners[state.ScannerIndex];
                var paramCount = scanner.ParameterPrompts.Length;
                var parameters = new string[scanner.ParameterPrompts.Length];

                scanner.Running = true;
                state.CurrentTutorial = scanner.Tutorial;

                onStateChanged(state);

                if (paramCount > 0)
                {
                    var skip = false;
                    for (var paramIndex = 0; paramIndex < paramCount; paramIndex++)
                    {
                        var (parameter, skipRequested) = requestParameter(scanner, paramIndex);
                        if (skipRequested)
                        {
                            skip = true;
                            break;
                        }
                        parameters[paramIndex] = parameter ?? "";
                    }

                    if (skip)
                    {
                        scanner.Running = false;
                        continue;
                    };
                }

                try
                {
                    await Task.Run(() => scanner.Opcode = this.scannerHost.Scan(pq, scanner.ScanDelegate, parameters,
                        scanner.PacketSource, ref this.skipped));
                }
                catch (FormatException) { }

                scanner.Running = false;

                if (this.stopped) return;
            }
        }

        public void Stop()
        {
            Skip();
            this.stopped = true;
        }

        public void Skip()
        {
            this.skipped = true;
        }

        private void OnMessageReceived(string connection, long epoch, byte[] data)
        {
            OnMessage(connection, epoch, data, PacketSource.Server);
        }

        private void OnMessageSent(string connection, long epoch, byte[] data)
        {
            OnMessage(connection, epoch, data, PacketSource.Client);
        }

        private void OnMessage(string connection, long epoch, byte[] data, PacketSource source)
        {
            pq.AddLast(new Packet
            {
                Connection = connection,
                Data = data,
                Source = source,
                Epoch = epoch,
            });
        }
    }
}