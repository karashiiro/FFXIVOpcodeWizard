using FFXIVOpcodeWizard.Models;
using Sapphire.Common.Network;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace FFXIVOpcodeWizard
{
    static class PacketScanner
    {
        /// <summary>
        /// Pull packets from the queue and do basic parsing on them.
        /// </summary>
        private static MetaPacket ScanGeneric(Packet basePacket)
        {
            var mp = new MetaPacket(basePacket)
            {
                PacketSize = BitConverter.ToUInt32(basePacket.Data, (int)Offsets.PacketSize),
                SegmentType = BitConverter.ToUInt16(basePacket.Data, (int)Offsets.SegmentType),
                Opcode = BitConverter.ToUInt16(basePacket.Data, (int)Offsets.IpcType),
            };

            return mp;
        }

        private static bool scanning = false;

        /// <summary>
        /// Returns the opcode of the first packet to meet the conditions outlined by del.
        /// </summary>
        public static ushort Scan(LinkedList<Packet> pq, Func<MetaPacket, string[], bool> del, string[] parameters, PacketDirection direction, out bool cancelled)
        {
            Console.CancelKeyPress += Console_CancelKeyPress;
            MetaPacket foundPacket;

            scanning = true;
            while (scanning)
            {
                while (pq.First == null)
                {
                    Thread.Sleep(2);
                    if (!scanning)
                    {
                        goto Cancelled;
                    }
                }

                if (pq.First.Value.Direction != direction)
                {
                    pq.RemoveFirst();
                    continue;
                }

                foundPacket = ScanGeneric(pq.First.Value);
                pq.RemoveFirst();

                Debug.Print($"{direction} => {foundPacket.Opcode:x4} - Length: {foundPacket.Data.Length}");

                if (del(foundPacket, parameters))
                {
                    scanning = false;
                    cancelled = false;
                    return foundPacket.Opcode;
                }
            }
Cancelled:
            cancelled = true;
            return 0;
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            if (scanning)
            {
                e.Cancel = true;
                scanning = false;
            }
        }
    }
}
