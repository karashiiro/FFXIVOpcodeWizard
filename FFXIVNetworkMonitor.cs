using System;
using System.Collections.Generic;
using Machina;
using Machina.FFXIV;

namespace FFXIVOpcodeWizard
{
    /// <summary>
    /// This is an edited version of Machina.FFXIV.FFXIVNetworkMonitor.
    /// Since the original class locks the WindowName to "FINAL FANTASY XIV", it's kind of complex to implement
    /// support for Chinese Server base on it, as the Chinese version of client uses a translated name.
    /// </summary>
    public class FFXIVNetworkMonitor
    {
        /// <summary>
        /// Specifies the type of monitor to use - Raw socket or WinPCap
        /// </summary>
        public TCPNetworkMonitor.NetworkMonitorType MonitorType
        { get; set; } = TCPNetworkMonitor.NetworkMonitorType.RawSocket;

        /// <summary>
        /// Specifies the region so that WindowName can be set properly
        /// </summary>
        public Region Region;

        /// <summary>
        /// Specifies the Process ID that is generating or receiving the traffic.  Either ProcessID or WindowName must be specified.
        /// </summary>
        public uint ProcessID
        { get; set; } = 0;

        /// <summary>
        /// Specifies the local IP address to override the detected IP
        /// </summary>
        public string LocalIP
        { get; set; } = "";

        /// <summary>
        /// Specifies whether to use Winsock/WinPcap server IP filtering instead of filtering in code
        ///   This has a small chance of losing data when new TCP sockets connect, but significantly reduces data processing overhead.
        /// </summary>
        public bool UseSocketFilter
        { get; set; } = false;

        #region Message Delegates section
        public delegate void MessageReceivedDelegate(string connection, long epoch, byte[] message);

        /// <summary>
        /// Specifies the delegate that is called when data is received and successfully decoded.
        /// </summary>
        public MessageReceivedDelegate MessageReceived = null;

        public void OnMessageReceived(string connection, long epoch, byte[] message)
        {
            MessageReceived?.Invoke(connection, epoch, message);
        }

        public delegate void MessageSentDelegate(string connection, long epoch, byte[] message);

        public MessageSentDelegate MessageSent = null;

        public void OnMessageSent(string connection, long epoch, byte[] message)
        {
            MessageSent?.Invoke(connection, epoch, message);
        }

        #endregion

        private TCPNetworkMonitor _monitor;
        private readonly Dictionary<string, FFXIVBundleDecoder> _sentDecoders = new Dictionary<string, FFXIVBundleDecoder>();
        private readonly Dictionary<string, FFXIVBundleDecoder> _receivedDecoders = new Dictionary<string, FFXIVBundleDecoder>();

        /// <summary>
        /// Validates the parameters and starts the monitor.
        /// </summary>
        public void Start()
        {
            if (_monitor != null)
            {
                _monitor.Stop();
                _monitor = null;
            }

            if (MessageReceived == null)
                throw new ArgumentException("MessageReceived delegate must be specified.");

            _monitor = new TCPNetworkMonitor
            {
                MonitorType = MonitorType,
                LocalIP = LocalIP,
                UseSocketFilter = UseSocketFilter,
                DataSent = ProcessSentMessage,
                DataReceived = ProcessReceivedMessage,
                ProcessID = ProcessID,
            };

            if (_monitor.ProcessID == 0)
                _monitor.WindowName = Region == Region.China ? "最终幻想XIV" : "FINAL FANTASY XIV";

            _monitor.Start();
        }

        /// <summary>
        /// Stops the monitor if it is active.
        /// </summary>
        public void Stop()
        {
            _monitor.DataSent = null;
            _monitor.DataReceived = null;
            _monitor?.Stop();
            _monitor = null;

            _sentDecoders.Clear();
            _receivedDecoders.Clear();
        }

        public void ProcessSentMessage(string connection, byte[] data)
        {
            Tuple<long, byte[]> message;
            if (!_sentDecoders.ContainsKey(connection))
                _sentDecoders.Add(connection, new FFXIVBundleDecoder());

            _sentDecoders[connection].StoreData(data);
            while ((message = _sentDecoders[connection].GetNextFFXIVMessage()) != null)
            {
                OnMessageSent(connection, message.Item1, message.Item2);
            }
        }

        public void ProcessReceivedMessage(string connection, byte[] data)
        {
            Tuple<long, byte[]> message;
            if (!_receivedDecoders.ContainsKey(connection))
                _receivedDecoders.Add(connection, new FFXIVBundleDecoder());

            _receivedDecoders[connection].StoreData(data);
            while ((message = _receivedDecoders[connection].GetNextFFXIVMessage()) != null)
            {
                OnMessageReceived(connection, message.Item1, message.Item2);
            }

        }
    }
}