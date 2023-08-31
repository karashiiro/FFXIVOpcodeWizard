﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using FFXIVOpcodeWizard.Models;
using Machina.FFXIV;
using Machina.Infrastructure;

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

        private bool aborted;
        private bool stopped;
        private bool skipped;

        private Queue<Packet> pq;

        /// <summary>
        /// Runs the detection program.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="skipCount"></param>
        /// <param name="onStateChanged">A callback function called each time a scan completes.</param>
        /// <param name="requestParameter">A function called when a parameter needs to be requested from the user.</param>
        /// <returns></returns>
        public async Task<bool> Run(Args args,
                                    int skipCount,
                                    Action<State> onStateChanged,
                                    Func<Scanner, int, (string parameter, bool skipRequested)> requestParameter)
        {
            this.aborted = false;
            this.stopped = false;
            this.skipped = false;

            this.pq = new Queue<Packet>();

            var state = new State
            {
                ScannerIndex = skipCount,
            };

            var monitor = BuildNetworkMonitor(args);
            monitor.Start();

            var scanners = args.Registry.AsList();

            for (; state.ScannerIndex < scanners.Count; state.ScannerIndex++)
            {
                var scanner = scanners[state.ScannerIndex];
                
                var parameters = new string[scanner.ParameterPrompts.Length];

                scanner.Opcode = 0;
                scanner.Running = true;
                state.CurrentTutorial = scanner.Tutorial;

                onStateChanged(state);

                if (parameters.Length > 0)
                {
                    var skip = false;
                    RequestParameters(scanner, parameters, requestParameter, ref skip);

                    if (skip)
                    {
                        scanner.Running = false;
                        continue;
                    };
                }

                await RunScanner(scanner, parameters);

                scanner.Running = false;

                if (this.stopped) return this.aborted;
            }

            return this.aborted;
        }

        public async Task<bool> RunOne(Scanner scanner,
                                       Args args,
                                       Action<State> onStateChanged,
                                       Func<Scanner, int, (string parameter, bool skipRequested)> requestParameter)
        {
            this.stopped = false;
            this.skipped = false;

            this.pq = new Queue<Packet>();

            var state = new State();

            var monitor = BuildNetworkMonitor(args);
            monitor.Start();

            var parameters = new string[scanner.ParameterPrompts.Length];

            scanner.Opcode = 0;
            scanner.Running = true;
            state.CurrentTutorial = scanner.Tutorial;

            onStateChanged(state);

            if (parameters.Length > 0)
            {
                var skip = false;
                RequestParameters(scanner, parameters, requestParameter, ref skip);

                if (skip)
                {
                    scanner.Running = false;
                    return this.aborted;
                };
            }

            await RunScanner(scanner, parameters);

            onStateChanged(state);

            scanner.Running = false;

            return this.aborted;
        }

        public void Abort()
        {
            this.aborted = true;
            Skip();
            Stop();
        }

        public void Stop()
        {
            this.stopped = true;
            Skip();
        }

        public void Skip()
        {
            this.skipped = true;
        }

        private FFXIVNetworkMonitor BuildNetworkMonitor(Args args)
        {
            var window = FindWindow("FFXIVGAME", null);
            GetWindowThreadProcessId(window, out var pid);
            var proc = Process.GetProcessById(Convert.ToInt32(pid));
            var gamePath = proc.MainModule?.FileName;
            
            var monitor = new FFXIVNetworkMonitor
            {
                MessageReceivedEventHandler = OnMessageReceived,
                MessageSentEventHandler = OnMessageSent,
                MonitorType = args.CaptureMode,
                WindowName = args.Region == Region.China ? "最终幻想XIV" : "FINAL FANTASY XIV",
                OodleImplementation = args.Region == Region.Korea ? Machina.FFXIV.Oodle.OodleImplementation.KoreanFfxivUdp : Machina.FFXIV.Oodle.OodleImplementation.FfxivTcp,
                UseDeucalion = args.CaptureMode == NetworkMonitorType.RawSocket,
                ProcessID = pid,
            };
            
            if (!string.IsNullOrEmpty(gamePath))
            {
                monitor.OodlePath = gamePath;
            }

            return monitor;
        }

        private Task RunScanner(Scanner scanner, string[] parameters)
        {
            return Task.Run(() =>
            {
                try
                {
                    scanner.Opcode = PacketScanner.Scan(this.pq, scanner, parameters, ref this.skipped, ref this.stopped);
                }
                catch (FormatException) { }
            });
        }

        private void OnMessageReceived(TCPConnection connection, long epoch, byte[] data)
        {
            OnMessage(connection.ToString(), epoch, data, PacketSource.Server);
        }

        private void OnMessageSent(TCPConnection connection, long epoch, byte[] data)
        {
            OnMessage(connection.ToString(), epoch, data, PacketSource.Client);
        }

        private void OnMessage(string connection, long epoch, byte[] data, PacketSource source)
        {
            lock(this.pq)
            {
                this.pq.Enqueue(new Packet
                {
                    Connection = connection,
                    Data = data,
                    Source = source,
                    Epoch = epoch,
                });
            }
        }
        
        private static void RequestParameters(Scanner scanner, IList<string> parameters, Func<Scanner, int, (string parameter, bool skipRequested)> requestParameter, ref bool skip)
        {
            for (var paramIndex = 0; paramIndex < parameters.Count; paramIndex++)
            {
                var (parameter, skipRequested) = requestParameter(scanner, paramIndex);
                if (skipRequested)
                {
                    skip = true;
                    break;
                }
                parameters[paramIndex] = parameter ?? "";
            }
        }
        
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        
        [DllImport("user32.dll", SetLastError=true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
    }
}