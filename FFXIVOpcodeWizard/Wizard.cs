using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

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
            ushort playerSetup = PacketProcessors.ScanPlayerSetup(pq, playerName);
            Console.WriteLine("PlayerSetup found at opcode 0x{0}!", playerSetup.ToString("X4"));
            output.Append("PlayerSetup: 0x").Append(playerSetup.ToString("X4")).Append(", // updated ").AppendLine(gamePatch);

            // ActorControl, ClientTrigger
            Console.WriteLine("Scanning for ActorControl, ClientTrigger. Please draw your weapon...");

            ushort clientTrigger = PacketProcessors.ScanClientTrigger(pq);
            Console.WriteLine("ClientTrigger found at opcode 0x{0}!", clientTrigger.ToString("X4"));
            output.Append("ClientTrigger: 0x").Append(clientTrigger.ToString("X4")).Append(", // updated ").AppendLine(gamePatch);

            ushort actorControl = PacketProcessors.ScanActorControl(pq);
            Console.WriteLine("ActorControl found at opcode 0x{0}!", actorControl.ToString("X4"));
            output.Append("ActorControl: 0x").Append(actorControl.ToString("X4")).Append(", // updated ").AppendLine(gamePatch);

            // Playtime
            Console.WriteLine("Scanning for Playtime. Please type /playtime...");
            ushort playtime = PacketProcessors.ScanPlaytime(pq);
            Console.WriteLine("Playtime found at opcode 0x{0}!", playtime.ToString("X4"));
            output.Append("Playtime: 0x").Append(playtime.ToString("X4")).Append(", // updated ").AppendLine(gamePatch);

            // MarketBoardSearchResult
            Console.WriteLine("Scanning for MarketBoardSearchResult. Please click \"Catalysts\" on the market board.");
            ushort marketBoardSearchResult = PacketProcessors.ScanMarketBoardSearchResult(pq);
            Console.WriteLine("MarketBoardSearchResult found at opcode 0x{0}!", marketBoardSearchResult.ToString("X4"));
            output.Append("MarketBoardSearchResult: 0x").Append(marketBoardSearchResult.ToString("X4")).Append(", // updated ").AppendLine(gamePatch);

            // MarketBoardItemListingCount, MarketBoardItemListing, MarketBoardItemListingHistory
            Console.WriteLine("Scanning for MarketBoardItemListingCount, MarketBoardItemListing, MarketBoardItemListingHistory. Please open the market board listings for Grade 7 Dark Matter...");

            ushort marketBoardItemListingCount = PacketProcessors.ScanMarketBoardItemListingCount(pq);
            Console.WriteLine("MarketBoardItemListingCount found at opcode 0x{0}!", marketBoardItemListingCount.ToString("X4"));
            output.Append("MarketBoardItemListingCount: 0x").Append(marketBoardItemListingCount.ToString("X4")).Append(", // updated ").AppendLine(gamePatch);

            ushort marketBoardItemListingHistory = PacketProcessors.ScanMarketBoardItemListingHistory(pq);
            Console.WriteLine("MarketBoardItemListingHistory found at opcode 0x{0}!", marketBoardItemListingHistory.ToString("X4"));
            output.Append("MarketBoardItemListingHistory: 0x").Append(marketBoardItemListingHistory.ToString("X4")).Append(", // updated ").AppendLine(gamePatch);

            ushort marketBoardItemListing = PacketProcessors.ScanMarketBoardItemListing(pq);
            Console.WriteLine("MarketBoardItemListing found at opcode 0x{0}!", marketBoardItemListing.ToString("X4"));
            output.Append("MarketBoardItemListing: 0x").Append(marketBoardItemListing.ToString("X4")).Append(", // updated ").AppendLine(gamePatch);
            
            // NpcSpawn
            Console.WriteLine("Scanning for NpcSpawn. Please enter your retainer name.");
            string retainerName = Console.ReadLine();
            Console.WriteLine("Please access this retainer from the retainer bell...");
            ushort npcSpawn = PacketProcessors.ScanNpcSpawn(pq, retainerName);
            Console.WriteLine("NpcSpawn found at opcode 0x{0}!", npcSpawn.ToString("X4"));
            output.Append("NpcSpawn: 0x").Append(npcSpawn.ToString("X4")).Append(", // updated ").AppendLine(gamePatch);

            // PlayerSpawn
            Console.WriteLine("Scanning for PlayerSpawn. Please enter your world ID.");
            ushort worldID = ushort.Parse(Console.ReadLine());
            Console.WriteLine("Please teleport to another zone or wait for another player to teleport in...");
            ushort playerSpawn = PacketProcessors.ScanPlayerSpawn(pq, worldID);
            Console.WriteLine("PlayerSpawn found at opcode 0x{0}!", playerSpawn.ToString("X4"));
            output.Append("PlayerSpawn: 0x").Append(playerSpawn.ToString("X4")).Append(", // updated ").AppendLine(gamePatch);
            
            // ItemInfo
            Console.WriteLine("Scanning for ItemInfo. Please teleport and open your chocobo saddlebag...");
            ushort itemInfo = PacketProcessors.ScanItemInfo(pq);
            Console.WriteLine("ItemInfo found at opcode 0x{0}!", itemInfo.ToString("X4"));
            output.Append("ItemInfo: 0x").Append(itemInfo.ToString("X4")).Append(", // updated ").AppendLine(gamePatch);
            
            // UpdateClassInfo
            Console.WriteLine("Scanning for UpdateClassInfo. Please enter the level of the job you will switch to.");
            ushort level = ushort.Parse(Console.ReadLine());
            Console.WriteLine("Please switch to that job...");
            ushort updateClassInfo = PacketProcessors.ScanUpdateClassInfo(pq, level);
            Console.WriteLine("UpdateClassInfo found at opcode 0x{0}!", updateClassInfo.ToString("X4"));
            output.Append("UpdateClassInfo: 0x").Append(updateClassInfo.ToString("X4")).Append(", // updated ").AppendLine(gamePatch);

            // InitZone
            Console.WriteLine("Scanning for InitZone. Please enter the TerritoryID of the zone you will teleport to.");
            ushort zoneID = ushort.Parse(Console.ReadLine());
            Console.WriteLine("Please teleport to that zone...");
            ushort initZone = PacketProcessors.ScanInitZone(pq, zoneID);
            Console.WriteLine("InitZone found at opcode 0x{0}!", initZone.ToString("X4"));
            output.Append("InitZone: 0x").Append(initZone.ToString("X4")).Append(", // updated ").AppendLine(gamePatch);

            // EventStart
            Console.WriteLine("Scanning for EventStart, EventPlay and EventFinish.");
            Console.WriteLine("Please begin fishing and put your rod away immediately.");
            ushort eventStart = PacketProcessors.ScanEventStart(pq);
            Console.WriteLine("EventStart found at opcode 0x{0}!", eventStart.ToString("X4"));
            output.Append("EventStart: 0x").Append(eventStart.ToString("X4")).Append(", // updated ").AppendLine(gamePatch);

            // EventPlay
            ushort eventPlay = PacketProcessors.ScanEventPlay(pq);
            Console.WriteLine("EventPlay found at opcode 0x{0}!", eventPlay.ToString("X4"));
            output.Append("EventPlay: 0x").Append(eventPlay.ToString("X4")).Append(", // updated ").AppendLine(gamePatch);

            // EventFinish
            ushort eventFinish = PacketProcessors.ScanEventFinish(pq);
            Console.WriteLine("EventFinish found at opcode 0x{0}!", eventFinish.ToString("X4"));
            output.Append("EventFinish: 0x").Append(eventFinish.ToString("X4")).Append(", // updated ").AppendLine(gamePatch);

            // EventUnk0 & EventUnk1
            Console.WriteLine("Scanning for EventUnk0 and EventUnk1. Please cast your line and catch a fish.");
            ushort eventUnk1 = PacketProcessors.ScanEventUnk1(pq);
            Console.WriteLine("EventUnk1 found at opcode 0x{0}!", eventUnk1.ToString("X4"));
            output.Append("EventUnk1: 0x").Append(eventUnk1.ToString("X4")).Append(", // updated ").AppendLine(gamePatch);
            ushort eventUnk0 = PacketProcessors.ScanEventUnk0(pq);
            Console.WriteLine("EventUnk0 found at opcode 0x{0}!", eventUnk0.ToString("X4"));
            output.Append("EventUnk0: 0x").Append(eventUnk0.ToString("X4")).Append(", // updated ").AppendLine(gamePatch);

            // Done
            Console.WriteLine("All packets found!\n\n");
            Console.WriteLine(output.ToString());
            Console.ReadLine();
        }
    }
}
