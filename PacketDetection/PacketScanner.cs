using FFXIVOpcodeWizard.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace FFXIVOpcodeWizard.PacketDetection
{
    public class PacketScanner
    {
        /// <summary>
        /// Pull packets from the queue and do basic parsing on them.
        /// </summary>
        private static IpcPacket ScanGeneric(Packet basePacket)
        {
            return new IpcPacket
            {
                Connection = basePacket.Connection,
                Data = basePacket.Data,
                Epoch = basePacket.Epoch,
                Source = basePacket.Source,
                PacketSize = BitConverter.ToUInt32(basePacket.Data, Offsets.PacketSize),
                SegmentType = BitConverter.ToUInt16(basePacket.Data, Offsets.SegmentType),
                Opcode = BitConverter.ToUInt16(basePacket.Data, Offsets.IpcType),
                SourceActor = BitConverter.ToUInt32(basePacket.Data, Offsets.SourceActor),
                TargetActor = BitConverter.ToUInt32(basePacket.Data, Offsets.TargetActor),
            };
        }

        /// <summary>
        /// Returns the opcode of the first packet to meet the conditions outlined by del.
        /// </summary>
        public static ushort Scan(Queue<Packet> pq, Scanner scanner, string[] parameters, ref bool skipped)
        {
            while (!skipped)
            {
                if (pq.Count == 0)
                    continue;
                Packet packet;
                lock (pq)
                {
                    packet = pq.Dequeue();
                }
                if (packet == null || packet.Source != scanner.PacketSource)
                    continue;

                var foundPacket = ScanGeneric(packet);

                Debug.Print($"{scanner.PacketSource} => {foundPacket.Opcode:x4} - Length: {foundPacket.Data.Length}");

                if (scanner.ScanDelegate(foundPacket, parameters, scanner.Comment))
                {
                    return foundPacket.Opcode;
                }
            }

            skipped = false;
            return 0;
        }
    }
}
