using Sapphire.Common.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace FFXIVOpcodeWizard
{
    static class Wizard
    {
        public static void Run(LinkedList<Packet> pq)
        {
            StringBuilder output = new StringBuilder();
            
            // Game Version
            Console.WriteLine("Please enter the current game version: ");
            Regex versionNameFilter = new Regex(@"[^0-9.]");
            string gamePatch = versionNameFilter.Replace(Console.ReadLine(), (match) => "");
            
            // PlayerSetup
            Console.WriteLine("Scanning for PlayerSetup. Please enter your character name.");
            string playerName = Console.ReadLine();
            Console.WriteLine("Please log in...");
            ushort playerSetup = ScanPlayerSetup(pq, playerName);
            Console.WriteLine("PlayerSetup found at opcode 0x{0}!", playerSetup.ToString("X4"));
            output.Append("PlayerSetup: 0x").Append(playerSetup.ToString("X4")).Append(", // updated ").AppendLine(gamePatch);
            
            // Playtime
            Console.WriteLine("Scanning for Playtime. Please type /playtime...");
            ushort playtime = ScanPlaytime(pq);
            Console.WriteLine("Playtime found at opcode 0x{0}!", playtime.ToString("X4"));
            output.Append("Playtime: 0x").Append(playtime.ToString("X4")).Append(", // updated ").AppendLine(gamePatch);

            // ActorControl, ClientTrigger
            Console.WriteLine("Scanning for ActorControl, ClientTrigger. Please draw your weapon...");

            ushort clientTrigger = ScanClientTrigger(pq);
            Console.WriteLine("ClientTrigger found at opcode 0x{0}!", clientTrigger.ToString("X4"));
            output.Append("ClientTrigger: 0x").Append(clientTrigger.ToString("X4")).Append(", // updated ").AppendLine(gamePatch);

            ushort actorControl = ScanActorControl(pq);
            Console.WriteLine("ActorControl found at opcode 0x{0}!", actorControl.ToString("X4"));
            output.Append("ActorControl: 0x").Append(actorControl.ToString("X4")).Append(", // updated ").AppendLine(gamePatch);

            // MarketBoardSearchResult
            Console.WriteLine("Scanning for MarketBoardSearchResult. Please click \"Catalysts\" on the market board.");
            ushort marketBoardSearchResult = ScanMarketBoardSearchResult(pq);
            Console.WriteLine("MarketBoardItemListingCount found at opcode 0x{0}!", marketBoardSearchResult.ToString("X4"));
            output.Append("MarketBoardItemListingCount: 0x").Append(marketBoardSearchResult.ToString("X4")).Append(", // updated ").AppendLine(gamePatch);

            // MarketBoardItemListingCount, MarketBoardItemListing, MarketBoardItemListingHistory
            Console.WriteLine("Scanning for MarketBoardItemListingCount, MarketBoardItemListing, MarketBoardItemListingHistory. Please open the market board listings for Grade 7 Dark Matter...");

            ushort marketBoardItemListingCount = ScanMarketBoardItemListingCount(pq);
            Console.WriteLine("MarketBoardItemListingCount found at opcode 0x{0}!", marketBoardItemListingCount.ToString("X4"));
            output.Append("MarketBoardItemListingCount: 0x").Append(marketBoardItemListingCount.ToString("X4")).Append(", // updated ").AppendLine(gamePatch);

            ushort marketBoardItemListingHistory = ScanMarketBoardItemListingHistory(pq);
            Console.WriteLine("MarketBoardItemListingHistory found at opcode 0x{0}!", marketBoardItemListingHistory.ToString("X4"));
            output.Append("MarketBoardItemListingHistory: 0x").Append(marketBoardItemListingHistory.ToString("X4")).Append(", // updated ").AppendLine(gamePatch);

            ushort marketBoardItemListing = ScanMarketBoardItemListing(pq);
            Console.WriteLine("MarketBoardItemListing found at opcode 0x{0}!", marketBoardItemListing.ToString("X4"));
            output.Append("MarketBoardItemListing: 0x").Append(marketBoardItemListing.ToString("X4")).Append(", // updated ").AppendLine(gamePatch);
            
            // NpcSpawn
            Console.WriteLine("Scanning for NpcSpawn. Please enter your retainer name.");
            string retainerName = Console.ReadLine();
            Console.WriteLine("Please access this retainer from the retainer bell...");
            ushort npcSpawn = ScanNpcSpawn(pq, retainerName);
            Console.WriteLine("NpcSpawn found at opcode 0x{0}!", npcSpawn.ToString("X4"));
            output.Append("NpcSpawn: 0x").Append(npcSpawn.ToString("X4")).Append(", // updated ").AppendLine(gamePatch);

            // PlayerSpawn
            Console.WriteLine("Scanning for PlayerSpawn. Please enter your world ID.");
            ushort worldID = ushort.Parse(Console.ReadLine());
            Console.WriteLine("Please teleport to another zone or wait for another player to teleport in...");
            ushort playerSpawn = ScanPlayerSpawn(pq, worldID);
            Console.WriteLine("PlayerSpawn found at opcode 0x{0}!", playerSpawn.ToString("X4"));
            output.Append("PlayerSpawn: 0x").Append(playerSpawn.ToString("X4")).Append(", // updated ").AppendLine(gamePatch);
            
            // ItemInfo
            Console.WriteLine("Scanning for ItemInfo. Please teleport and open your chocobo saddlebag...");
            ushort itemInfo = ScanItemInfo(pq);
            Console.WriteLine("ItemInfo found at opcode 0x{0}!", itemInfo.ToString("X4"));
            output.Append("ItemInfo: 0x").Append(itemInfo.ToString("X4")).Append(", // updated ").AppendLine(gamePatch);
            
            // UpdateClassInfo
            Console.WriteLine("Scanning for UpdateClassInfo. Please enter the level of the job you will switch to.");
            ushort level = ushort.Parse(Console.ReadLine());
            Console.WriteLine("Please switch to that job...");
            ushort updateClassInfo = ScanUpdateClassInfo(pq, level);
            Console.WriteLine("UpdateClassInfo found at opcode 0x{0}!", updateClassInfo.ToString("X4"));
            output.Append("UpdateClassInfo: 0x").Append(updateClassInfo.ToString("X4")).Append(", // updated ").AppendLine(gamePatch);

            // Done
            Console.WriteLine("All packets found!\n\n");
            Console.WriteLine(output.ToString());
            Console.ReadLine();
        }
        

        /*
         * From here down are our helper methods to ID packets.
         */

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

        private static ushort ScanPlaytime(LinkedList<Packet> pq)
        {
            MetaPacket foundPacket = null;
            while (foundPacket == null ||
                foundPacket.Direction == "outbound")
            {
                MetaPacket temp = ScanGeneric(pq);
                if (temp.PacketSize == 40)
                {
                    foundPacket = temp;
                    break;
                }
            }
            return foundPacket.Opcode;
        }

        private static ushort ScanActorControl(LinkedList<Packet> pq)
        {
            MetaPacket foundPacket = null;
            while (foundPacket == null ||
                foundPacket.Direction == "outbound")
            {
                MetaPacket temp = ScanGeneric(pq);
                if (temp.PacketSize == 56 && BitConverter.ToUInt32(temp.Data, (int)Offsets.IpcData + 4) == 1)
                {
                    foundPacket = temp;
                    break;
                }
            }
            return foundPacket.Opcode;
        }

        private static ushort ScanMarketBoardItemListingCount(LinkedList<Packet> pq)
        {
            MetaPacket foundPacket = null;
            while (foundPacket == null ||
                foundPacket.Direction == "outbound")
            {
                MetaPacket temp = ScanGeneric(pq);
                if (temp.PacketSize == 0x30 && BitConverter.ToUInt32(temp.Data, (int)Offsets.IpcData) == 17837)
                {
                    foundPacket = temp;
                    break;
                }
            }
            return foundPacket.Opcode;
        }

        private static ushort ScanMarketBoardItemListing(LinkedList<Packet> pq)
        {
            MetaPacket foundPacket = null;
            while (foundPacket == null ||
                foundPacket.Direction == "outbound")
            {
                MetaPacket temp = ScanGeneric(pq);
                if ((int)Offsets.IpcData + 1520 < temp.PacketSize && BitConverter.ToUInt32(temp.Data, (int)Offsets.IpcData + 44) == 17837)
                {
                    foundPacket = temp;
                    break;
                }
            }
            return foundPacket.Opcode;
        }

        private static ushort ScanMarketBoardItemListingHistory(LinkedList<Packet> pq)
        {
            MetaPacket foundPacket = null;
            while (foundPacket == null ||
                foundPacket.Direction == "outbound")
            {
                MetaPacket temp = ScanGeneric(pq);
                if (temp.PacketSize == 1080 && BitConverter.ToUInt32(temp.Data, (int)Offsets.IpcData) == 17837)
                {
                    foundPacket = temp;
                    break;
                }
            }
            return foundPacket.Opcode;
        }

        private static ushort ScanMarketBoardSearchResult(LinkedList<Packet> pq)
        {
            MetaPacket foundPacket = null;
            while (foundPacket == null ||
                foundPacket.Direction == "outbound")
            {
                MetaPacket temp = ScanGeneric(pq);
                if (temp.PacketSize == 208 && BitConverter.ToUInt32(temp.Data, (int)Offsets.IpcData + 56) == 17837)
                {
                    foundPacket = temp;
                    break;
                }
            }
            return foundPacket.Opcode;
        }

        private static ushort ScanNpcSpawn(LinkedList<Packet> pq, string npcName)
        {
            MetaPacket foundPacket = null;
            while (foundPacket == null ||
                foundPacket.Direction == "outbound")
            {
                MetaPacket temp = ScanGeneric(pq);
                if (temp.PacketSize > 592 && Encoding.UTF8.GetString(temp.Data.Skip(592).Take(npcName.Length).ToArray()) == npcName)
                {
                    foundPacket = temp;
                    break;
                }
            }
            return foundPacket.Opcode;
        }

        private static ushort ScanItemInfo(LinkedList<Packet> pq)
        {
            MetaPacket foundPacket = null;
            while (foundPacket == null ||
                foundPacket.Direction == "outbound")
            {
                MetaPacket temp = ScanGeneric(pq);
                if (temp.PacketSize == 96 && BitConverter.ToUInt16(temp.Data, (int)Offsets.IpcData + 8) == 4000)
                {
                    foundPacket = temp;
                    break;
                }
            }
            return foundPacket.Opcode;
        }

        private static ushort ScanPlayerSpawn(LinkedList<Packet> pq, ushort worldID)
        {
            MetaPacket foundPacket = null;
            while (foundPacket == null ||
                foundPacket.Direction == "outbound")
            {
                MetaPacket temp = ScanGeneric(pq);
                if (temp.PacketSize > 500 && BitConverter.ToUInt16(temp.Data, (int)Offsets.IpcData + 4) == worldID)
                {
                    foundPacket = temp;
                    break;
                }
            }
            return foundPacket.Opcode;
        }

        private static ushort ScanPlayerSetup(LinkedList<Packet> pq, string playerName)
        {
            MetaPacket foundPacket = null;
            while (foundPacket == null ||
                foundPacket.Direction == "outbound")
            {
                MetaPacket temp = ScanGeneric(pq);
                if (temp.PacketSize > 300 && Encoding.UTF8.GetString(temp.Data).IndexOf(playerName) != -1)
                {
                    foundPacket = temp;
                    break;
                }
            }
            return foundPacket.Opcode;
        }

        private static ushort ScanUpdateClassInfo(LinkedList<Packet> pq, ushort level)
        {
            MetaPacket foundPacket = null;
            while (foundPacket == null ||
                foundPacket.Direction == "outbound")
            {
                MetaPacket temp = ScanGeneric(pq);
                if (temp.PacketSize == 48 && BitConverter.ToUInt16(temp.Data, (int)Offsets.IpcData + 4) == level)
                {
                    foundPacket = temp;
                    break;
                }
            }
            return foundPacket.Opcode;
        }


        private static ushort ScanClientTrigger(LinkedList<Packet> pq)
        {
            MetaPacket foundPacket = null;
            while (foundPacket == null ||
                foundPacket.Direction == "inbound")
            {
                MetaPacket temp = ScanGeneric(pq);
                if (temp.PacketSize == 64 && BitConverter.ToUInt32(temp.Data, (int)Offsets.IpcData) == 1)
                {
                    foundPacket = temp;
                    break;
                }
            }
            return foundPacket.Opcode;
        }
    }
}
