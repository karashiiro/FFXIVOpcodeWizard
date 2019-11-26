using Sapphire.Common.Network;
using System;
using System.Collections.Generic;
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
        private static MetaPacket ScanGeneric(LinkedList<Packet> pq)
        {
            while (pq.First == null)
            {
                Thread.Sleep(2);
            }

            Packet basePacket = pq.First();
            pq.RemoveFirst();
            MetaPacket mp = new MetaPacket(basePacket)
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
        private static ushort ScanInbound(LinkedList<Packet> pq, Func<MetaPacket, bool> del)
        {
            MetaPacket foundPacket = null;
            while (foundPacket == null ||
                foundPacket.Direction == "outbound")
            {
                MetaPacket temp = ScanGeneric(pq);
                if (del(temp))
                {
                    foundPacket = temp;
                    break;
                }
            }
            return foundPacket.Opcode;
        }

        /// <summary>
        /// Returns the opcode of the first outbound packet to meet the conditions outlined by del.
        /// </summary>
        /// <param name="pq"></param>
        /// <param name="del"></param>
        /// <returns></returns>
        private static ushort ScanOutbound(LinkedList<Packet> pq, Func<MetaPacket, bool> del)
        {
            MetaPacket foundPacket = null;
            while (foundPacket == null ||
                foundPacket.Direction == "inbound")
            {
                MetaPacket temp = ScanGeneric(pq);
                if (del(temp))
                {
                    foundPacket = temp;
                    break;
                }
            }
            return foundPacket.Opcode;
        }

        public static ushort ScanPlaytime(LinkedList<Packet> pq)
        {
            return ScanInbound(pq, (packet) => packet.PacketSize == 40);
        }

        public static ushort ScanActorControl(LinkedList<Packet> pq)
        {
            return ScanInbound(pq, (packet) => packet.PacketSize == 56 && BitConverter.ToUInt32(packet.Data, (int)Offsets.IpcData + 4) == 1);
        }

        public static ushort ScanMarketBoardItemListingCount(LinkedList<Packet> pq)
        {
            return ScanInbound(pq, (packet) => packet.PacketSize == 48 && BitConverter.ToUInt32(packet.Data, (int)Offsets.IpcData) == 17837);
        }

        public static ushort ScanMarketBoardItemListing(LinkedList<Packet> pq)
        {
            return ScanInbound(pq, (packet) => packet.PacketSize > 1552 && BitConverter.ToUInt32(packet.Data, (int)Offsets.IpcData + 44) == 17837);
        }

        public static ushort ScanMarketBoardItemListingHistory(LinkedList<Packet> pq)
        {
            return ScanInbound(pq, (packet) => packet.PacketSize == 1080 && BitConverter.ToUInt32(packet.Data, (int)Offsets.IpcData) == 17837);
        }

        public static ushort ScanMarketBoardSearchResult(LinkedList<Packet> pq)
        {
            return ScanInbound(pq, (packet) => packet.PacketSize == 208 && BitConverter.ToUInt32(packet.Data, (int)Offsets.IpcData + 56) == 17837);
        }

        public static ushort ScanNpcSpawn(LinkedList<Packet> pq, string npcName)
        {
            return ScanInbound(pq, (packet) => packet.PacketSize > 592 && Encoding.UTF8.GetString(packet.Data.Skip(592).Take(npcName.Length).ToArray()) == npcName);
        }

        public static ushort ScanItemInfo(LinkedList<Packet> pq)
        {
            return ScanInbound(pq, (packet) => packet.PacketSize == 96 && BitConverter.ToUInt16(packet.Data, (int)Offsets.IpcData + 8) == 4000);
        }

        public static ushort ScanPlayerSpawn(LinkedList<Packet> pq, ushort worldID)
        {
            return ScanInbound(pq, (packet) => packet.PacketSize > 500 && BitConverter.ToUInt16(packet.Data, (int)Offsets.IpcData + 4) == worldID);
        }

        public static ushort ScanPlayerSetup(LinkedList<Packet> pq, string playerName)
        {
            return ScanInbound(pq, (packet) => packet.PacketSize > 300 && Encoding.UTF8.GetString(packet.Data).IndexOf(playerName) != -1);
        }

        public static ushort ScanUpdateClassInfo(LinkedList<Packet> pq, ushort level)
        {
            return ScanInbound(pq, (packet) => packet.PacketSize == 48 && BitConverter.ToUInt16(packet.Data, (int)Offsets.IpcData + 4) == level);
        }


        public static ushort ScanClientTrigger(LinkedList<Packet> pq)
        {
            return ScanOutbound(pq, (packet) => packet.PacketSize == 64 && BitConverter.ToUInt32(packet.Data, (int)Offsets.IpcData) == 1);
        }
    }
}
