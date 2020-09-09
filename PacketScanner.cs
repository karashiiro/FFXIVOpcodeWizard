using FFXIVOpcodeWizard.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace FFXIVOpcodeWizard
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
        public ushort Scan(LinkedList<Packet> pq, Func<MetaPacket, string[], bool> del, string[] parameters, PacketSource source, ref bool skipped)
        {
            while (!skipped)
            {
                while (pq.First == null)
                {
                    Thread.Sleep(2);
                    if (skipped)
                    {
                        goto Cancelled;
                    }
                }

                if (pq.First.Value.Source != source)
                {
                    pq.RemoveFirst();
                    continue;
                }

                var foundPacket = ScanGeneric(pq.First.Value);
                pq.RemoveFirst();

                Debug.Print($"{source} => {foundPacket.Opcode:x4} - Length: {foundPacket.Data.Length}");

                if (del(foundPacket, parameters))
                {
                    return foundPacket.Opcode;
                }
            }

            Cancelled:
            skipped = false;
            return 0;
        }
    }
}
