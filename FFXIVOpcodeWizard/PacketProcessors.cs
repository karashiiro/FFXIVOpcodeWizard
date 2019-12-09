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
        private static ushort ScanInbound(LinkedList<Packet> pq, Func<MetaPacket, bool> del)
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

                if (del(foundPacket))
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
        private static ushort ScanOutbound(LinkedList<Packet> pq, Func<MetaPacket, bool> del)
        {
            MetaPacket foundPacket;
            while (true)
            {
                while (pq.First == null || pq.First.Value.Direction != "outbound")
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

                if (del(foundPacket))
                    break;
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

        public static ushort ScanInitZone(LinkedList<Packet> pq, ushort zoneID)
        {
            return ScanInbound(pq, (packet) => packet.PacketSize == 128 &&  BitConverter.ToUInt16(packet.Data, (int)Offsets.IpcData + 2) == zoneID);
        }

        public static ushort ScanEventPlay(LinkedList<Packet> pq)
        {
            return ScanInbound(pq, (packet) => packet.PacketSize == 72 && BitConverter.ToUInt32(packet.Data, (int) Offsets.IpcData + 8) == 0x150001 );
        }

        public static ushort ScanEventStart(LinkedList<Packet> pq)
        {
            return ScanInbound(pq, (packet) => packet.PacketSize == 56 && BitConverter.ToUInt32(packet.Data, (int)Offsets.IpcData + 8) == 0x150001);
        }

        public static ushort ScanEventFinish(LinkedList<Packet> pq)
        {
            return ScanInbound(pq, (packet) => packet.PacketSize == 48 && BitConverter.ToUInt32(packet.Data, (int)Offsets.IpcData) == 0x150001 && packet.Data[(int)Offsets.IpcData + 4] == 0x14 && packet.Data[(int)Offsets.IpcData + 5] == 0x01);
        }

        public static ushort ScanEventUnk0(LinkedList<Packet> pq)
        {
            return ScanInbound(pq, (packet) => packet.PacketSize == 80 && BitConverter.ToUInt32(packet.Data, (int)Offsets.IpcData + 0x1C) == 284);
        }

        public static ushort ScanEventUnk1(LinkedList<Packet> pq)
        {
            return ScanInbound(pq, (packet) => packet.PacketSize == 56 && BitConverter.ToUInt32(packet.Data, (int)Offsets.IpcData + 0x08) == 257);
        }

        public static ushort ScanUseMooch(LinkedList<Packet> pq)
        {
            return ScanInbound(pq, (packet) => packet.PacketSize == 80 && BitConverter.ToUInt32(packet.Data, (int)Offsets.IpcData + 0x18) == 2587);
        }

        public static ushort ScanCfPreferredRole(LinkedList<Packet> pq)
        {
            return ScanInbound(pq, (packet) =>
            {
                if (packet.PacketSize != 48)
                    return false;

                var allInRange = true;

                for (var i = 1; i < 10; i++)
                {
                    if (packet.Data[(int)Offsets.IpcData + i] > 4 || packet.Data[(int)Offsets.IpcData + i] < 1)
                        allInRange = false;
                }

                return allInRange;
            });
        }

        public static ushort ScanCfNotifyPop(LinkedList<Packet> pq)
        {
            return ScanInbound(pq, (packet) => packet.PacketSize == 64 && packet.Data[(int)Offsets.IpcData + 20] == 0x22);
        }
    }
}
