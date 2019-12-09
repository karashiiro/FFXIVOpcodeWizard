using Sapphire.Common.Network;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace FFXIVOpcodeWizard
{
    static class PacketProcessors
    {
        /// <summary>
        /// Pull packets from the queue and do basic parsing on them.
        /// </summary>
        /// <param name="pq"></param>
        /// <returns></returns>
        private static MetaPacket ScanGeneric(Packet basePacket)
        {
            var mp = new MetaPacket(basePacket)
            {
                PacketSize = BitConverter.ToUInt32(basePacket.Data, (int)Offsets.PacketSize),
                SegmentType = BitConverter.ToUInt16(basePacket.Data, (int)Offsets.SegmentType),
                Opcode = BitConverter.ToUInt16(basePacket.Data, (int)Offsets.IpcType)
            };

            return mp;
        }

        /// <summary>
        /// Returns the opcode of the first inbound packet to meet the conditions outlined by del.
        /// </summary>
        /// <param name="pq"></param>
        /// <param name="del"></param>
        /// <returns></returns>
        public static ushort ScanInbound(LinkedList<Packet> pq, Func<MetaPacket, string[], bool> del, string[] parameters)
        {
            MetaPacket foundPacket;
            while (true)
            {
                while (pq.First == null)
                {
                    Thread.Sleep(2);
                }

                if (pq.First.Value.Direction == "outbound")
                {
                    pq.RemoveFirst();
                    continue;
                }

                foundPacket = ScanGeneric(pq.First(p => p.Direction == "inbound"));
                pq.RemoveFirst();

                Debug.Print($"RECV => {foundPacket.Opcode:x} - {foundPacket.Data.Length}");

                if (del(foundPacket, parameters))
                    break;
            }
            return foundPacket.Opcode;
        }

        /// <summary>
        /// Returns the opcode of the first outbound packet to meet the conditions outlined by del.
        /// </summary>
        /// <param name="pq"></param>
        /// <param name="del"></param>
        /// <returns></returns>
        public static ushort ScanOutbound(LinkedList<Packet> pq, Func<MetaPacket, string[], bool> del, string[] parameters)
        {
            MetaPacket foundPacket;
            while (true)
            {
                while (pq.First == null)
                {
                    Thread.Sleep(2);
                }

                if (pq.First.Value.Direction == "inbound")
                {
                    pq.RemoveFirst();
                    continue;
                }

                foundPacket = ScanGeneric(pq.First(p => p.Direction == "outbound"));
                pq.RemoveFirst();

                Debug.Print($"SEND => {foundPacket.Opcode:x} - {foundPacket.Data.Length}");

                if (del(foundPacket, parameters))
                    break;
            }
            return foundPacket.Opcode;
        }
    }
}
