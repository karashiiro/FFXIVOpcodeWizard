using FFXIVOpcodeWizard.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace FFXIVOpcodeWizard.PacketDetection
{
    public class PacketScanner
    {
        /// <summary>
        /// Pull packets from the queue and do basic parsing on them.
        /// </summary>
        private static MetaPacket ScanGeneric(Packet basePacket)
        {
            return new MetaPacket
            {
                Connection = basePacket.Connection,
                Data = basePacket.Data,
                Epoch = basePacket.Epoch,
                Source = basePacket.Source,
                PacketSize = BitConverter.ToUInt32(basePacket.Data, (int)Offsets.PacketSize),
                SegmentType = BitConverter.ToUInt16(basePacket.Data, (int)Offsets.SegmentType),
                Opcode = BitConverter.ToUInt16(basePacket.Data, (int)Offsets.IpcType),
            };
        }

        /// <summary>
        /// Returns the opcode of the first packet to meet the conditions outlined by del.
        /// </summary>
        public static ushort Scan(Queue<Packet> pq, Func<MetaPacket, string[], bool> del, string[] parameters, PacketSource source, ref bool skipped)
        {
            while (!skipped)
            {
                if (pq.Count == 0)
                    continue;
                var packet = pq.Dequeue();
                if (packet == null || packet.Source != source)
                    continue;

                var foundPacket = ScanGeneric(packet);

                Debug.Print($"{source} => {foundPacket.Opcode:x4} - Length: {foundPacket.Data.Length}");

                if (del(foundPacket, parameters))
                {
                    return foundPacket.Opcode;
                }
            }

            skipped = false;
            return 0;
        }
    }
}
