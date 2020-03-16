using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using FFXIVOpcodeWizard.Models;
using Sapphire.Common.Network;

namespace FFXIVOpcodeWizard
{ 
    class WizardProcessor
    {
        private readonly Queue<PacketWizard> wizards = new Queue<PacketWizard>();

        public WizardProcessor()
        {
            Setup();
        }

        private bool IncludeBytes(byte[] target, byte[] search)
        {
            for (int i = 0; i < target.Length - search.Length; ++i)
            {
                bool result = true;
                for (int j = 0; j < search.Length; ++j)
                {
                    if (search[j] != target[i + j])
                    {
                        result = false;
                        break;
                    }
                }

                if (result)
                {
                    return true;
                }
            }

            return false;
        }

        private void Setup()
        {
            //=================
            RegisterPacketWizard("PlayerSetup", "Please enter your character name and log in.", PacketDirection.Server,
                (packet, parameters) => packet.PacketSize > 300 && IncludeBytes(packet.Data, Encoding.UTF8.GetBytes(parameters[0])), 1);

            //=================
            RegisterPacketWizard("UpdateHpMpTp", "Enter your max HP, then alter your HP or MP and allow your stats to regenerate completely.", PacketDirection.Server,
                (packet, parameters) => packet.PacketSize == 48 &&
                    BitConverter.ToUInt32(packet.Data, (int) Offsets.IpcData).ToString() == parameters[0] && // HP equals MaxHP
                    BitConverter.ToUInt16(packet.Data, (int) Offsets.IpcData + 4) == 10000, 1); // MP equals 10000
            //=================
            RegisterPacketWizard("ClientTrigger", "Please draw your weapon.", PacketDirection.Client,
                (packet, _) =>
                    packet.PacketSize == 64 && BitConverter.ToUInt32(packet.Data, (int) Offsets.IpcData) == 1);
            RegisterPacketWizard("ActorControl", string.Empty, PacketDirection.Server,
                (packet, _) => packet.PacketSize == 56 &&
                               BitConverter.ToUInt32(packet.Data, (int) Offsets.IpcData + 4) == 1);
            //=================
            RegisterPacketWizard("ActorControlSelf", "Please enter sanctuary and wait for rested bonus gains", PacketDirection.Server,
                (packet, _) => packet.PacketSize == 64 &&
                               BitConverter.ToUInt16(packet.Data, (int)Offsets.IpcData) == 24 &&
                               BitConverter.ToUInt32(packet.Data, (int)Offsets.IpcData + 4) <= 604800 &&
                               BitConverter.ToUInt32(packet.Data, (int)Offsets.IpcData + 8) == 0 &&
                               BitConverter.ToUInt32(packet.Data, (int)Offsets.IpcData + 12) == 0 &&
                               BitConverter.ToUInt32(packet.Data, (int)Offsets.IpcData + 16) == 0 &&
                               BitConverter.ToUInt32(packet.Data, (int)Offsets.IpcData + 20) == 0 &&
                               BitConverter.ToUInt32(packet.Data, (int)Offsets.IpcData + 24) == 0);
            //=================
            RegisterPacketWizard("ChatHandler", "Please enter a message, and then /say it in-game...", PacketDirection.Client,
                (packet, parameters) => IncludeBytes(packet.Data, Encoding.UTF8.GetBytes(parameters[0])), 1);
            //=================
            RegisterPacketWizard("Playtime", "Please quickly type /playtime...", PacketDirection.Server,
                (packet, _) => packet.PacketSize == 40);
            //=================
            byte[] searchBytes = null;
            RegisterPacketWizard("SetSearchInfoHandler", "Please enter a somewhat lengthy search message here, and then set it in-game...", PacketDirection.Client,
                (packet, parameters) =>
                {
                    if (searchBytes == null)
                    {
                        searchBytes = Encoding.UTF8.GetBytes(parameters[0]);
                    }
                    return IncludeBytes(packet.Data, searchBytes);
                }, 1);
            RegisterPacketWizard("UpdateSearchInfo", string.Empty, PacketDirection.Server,
                (packet, _) => IncludeBytes(packet.Data, searchBytes));
            RegisterPacketWizard("ExamineSearchInfo", "Close the search information editor, and then open your search information with the \"View Search Info\" button...", PacketDirection.Server,
                (packet, _) => packet.PacketSize > 232 && IncludeBytes(packet.Data, searchBytes));
            //=================
            RegisterPacketWizard("Examine", "Please enter a nearby character's name, and then examine their equipment...", PacketDirection.Server,
                (packet, parameters) => packet.PacketSize == 1016 && IncludeBytes(packet.Data, Encoding.UTF8.GetBytes(parameters[0])), 1);
            //=================
            int marketBoardItemDetectionId = 17837;
            RegisterPacketWizard("MarketBoardSearchResult", "Please click \"Catalysts\" on the market board.",
                PacketDirection.Server,
                (packet, _) => packet.PacketSize == 208 &&
                               BitConverter.ToUInt32(packet.Data, (int) Offsets.IpcData + 56) == marketBoardItemDetectionId);
            RegisterPacketWizard("MarketBoardItemListingCount",
                "Please open the market board listings for Grade 7 Dark Matter...", PacketDirection.Server,
                (packet, _) => packet.PacketSize == 48 &&
                               BitConverter.ToUInt32(packet.Data, (int) Offsets.IpcData) == marketBoardItemDetectionId);
            RegisterPacketWizard("MarketBoardItemListingHistory", string.Empty, PacketDirection.Server,
                (packet, _) => packet.PacketSize == 1080 &&
                               BitConverter.ToUInt32(packet.Data, (int) Offsets.IpcData) == marketBoardItemDetectionId);
            RegisterPacketWizard("MarketBoardItemListing", string.Empty, PacketDirection.Server,
                (packet, _) => packet.PacketSize > 1552 &&
                               BitConverter.ToUInt32(packet.Data, (int) Offsets.IpcData + 44) == marketBoardItemDetectionId);
            //=================
            RegisterPacketWizard("MarketTaxRates",
                "Please visit a retainer counter and request information about market tax rates...", PacketDirection.Server,
                (packet, _) =>
                {
                    if (packet.PacketSize != 72)
                        return false;

                    var rate1 = BitConverter.ToUInt32(packet.Data, (int) Offsets.IpcData + 8);
                    var rate2 = BitConverter.ToUInt32(packet.Data, (int)Offsets.IpcData + 12);
                    var rate3 = BitConverter.ToUInt32(packet.Data, (int)Offsets.IpcData + 16);
                    var rate4 = BitConverter.ToUInt32(packet.Data, (int)Offsets.IpcData + 20);

                    return (rate1 >= 0 && rate1 <= 7) && (rate2 >= 0 && rate2 <= 7) && (rate3 >= 0 && rate3 <= 7) &&
                           (rate4 >= 0 && rate4 <= 7);
                });
            //=================
            RegisterPacketWizard("NpcSpawn", "Scanning for NpcSpawn. Please enter your retainer name.",
                PacketDirection.Server,
                (packet, parameters) => packet.PacketSize > 624 && 
                    IncludeBytes(packet.Data.Skip(588).Take(32).ToArray(), Encoding.UTF8.GetBytes(parameters[0])), 1);
            //=================
            RegisterPacketWizard("PlayerSpawn", "Scanning for PlayerSpawn. Please enter your world ID.",
                PacketDirection.Server, (packet, parameters) =>
                    packet.PacketSize > 500 && BitConverter.ToUInt16(packet.Data, (int) Offsets.IpcData + 4) ==
                    int.Parse(parameters[0]), 1);
            //=================
            RegisterPacketWizard("ItemInfo", "Please teleport and open your chocobo saddlebag...",
                PacketDirection.Server,
                (packet, _) => packet.PacketSize == 96 &&
                               BitConverter.ToUInt16(packet.Data, (int) Offsets.IpcData + 8) == 4000);
            //=================
            RegisterPacketWizard("UpdateClassInfo",
                "Scanning for UpdateClassInfo. Please enter the level of the job you will switch to and switch to it.",
                PacketDirection.Server, (packet, parameters) =>
                    packet.PacketSize == 48 && BitConverter.ToUInt16(packet.Data, (int) Offsets.IpcData + 4) ==
                    int.Parse(parameters[0]), 1);
            //=================
            RegisterPacketWizard("CurrencyCrystalInfo", "Please enter the number of Lightning Crystals you have, and then teleport to New Gridania.", PacketDirection.Server,
                (packet, parameters) => packet.PacketSize == 64 &&
                    BitConverter.ToUInt32(packet.Data, (int) Offsets.IpcData + 0x8) == int.Parse(parameters[0]), 1);
            //=================
            RegisterPacketWizard("InitZone", string.Empty, PacketDirection.Server,
                (packet, _) => packet.PacketSize == 128 &&
                               BitConverter.ToUInt16(packet.Data, (int) Offsets.IpcData + 2) == 132);
            //=================
            RegisterPacketWizard("EventStart", "Please begin fishing and put your rod away immediately",
                PacketDirection.Server,
                (packet, _) => packet.PacketSize == 56 &&
                               BitConverter.ToUInt32(packet.Data, (int) Offsets.IpcData + 8) == 0x150001);
            RegisterPacketWizard("EventPlay", string.Empty, PacketDirection.Server,
                (packet, _) => packet.PacketSize == 72 &&
                               BitConverter.ToUInt32(packet.Data, (int) Offsets.IpcData + 8) == 0x150001);
            RegisterPacketWizard("EventFinish", string.Empty, PacketDirection.Server,
                (packet, _) => packet.PacketSize == 48 &&
                               BitConverter.ToUInt32(packet.Data, (int) Offsets.IpcData) == 0x150001 &&
                               packet.Data[(int) Offsets.IpcData + 4] == 0x14 &&
                               packet.Data[(int) Offsets.IpcData + 5] == 0x01);
            //=================
            RegisterPacketWizard("SomeDirectorUnk4", "Please cast your line and catch a fish.", PacketDirection.Server,
                (packet, _) => packet.PacketSize == 56 &&
                               BitConverter.ToUInt32(packet.Data, (int) Offsets.IpcData + 0x08) == 257);
            RegisterPacketWizard("EventPlay4", string.Empty, PacketDirection.Server,
                (packet, _) => packet.PacketSize == 80 &&
                               BitConverter.ToUInt32(packet.Data, (int) Offsets.IpcData + 0x1C) == 284);
            //=================
            RegisterPacketWizard("UpdateInventorySlot", "Please purchase a Pill Bug to use as bait.",
                PacketDirection.Server,
                (packet, _) => packet.PacketSize == 96 &&
                               BitConverter.ToUInt32(packet.Data, (int) Offsets.IpcData + 0x10) == 2587);
            //=================
            RegisterPacketWizard("UseMooch", "Please catch a moochable 'Harbor Herring' from Mist using Pill Bug bait.",
                PacketDirection.Server,
                (packet, _) => packet.PacketSize == 80 &&
                               BitConverter.ToUInt32(packet.Data, (int) Offsets.IpcData + 0x18) == 2587);
            //=================
            RegisterPacketWizard("CFPreferredRole", "Please wait.", PacketDirection.Server, (packet, _) =>
            {
                if (packet.PacketSize != 48)
                    return false;

                var allInRange = true;

                for (var i = 1; i < 10; i++)
                    if (packet.Data[(int) Offsets.IpcData + i] > 4 || packet.Data[(int) Offsets.IpcData + i] < 1)
                        allInRange = false;

                return allInRange;
            });
            //=================
            RegisterPacketWizard("CFNotify", "Please queue for \"The Vault\" as an undersized party.", // CFNotifyPop
                PacketDirection.Server,
                (packet, _) => packet.PacketSize == 64 && packet.Data[(int)Offsets.IpcData + 20] == 0x22);
            //=================
            RegisterPacketWizard("ActorSetPos", "Please find an Aetheryte and teleport to Mist East.",
                PacketDirection.Server,
                (packet, _) => {
                    if (packet.PacketSize != 56) return false;

                    var x = BitConverter.ToSingle(packet.Data, (int)Offsets.IpcData + 8);
                    var y = BitConverter.ToSingle(packet.Data, (int)Offsets.IpcData + 12);
                    var z = BitConverter.ToSingle(packet.Data, (int)Offsets.IpcData + 16);

                    return Math.Abs(x - 85) < 15 && Math.Abs(z + 14) < 15 && Math.Abs(y - 18) < 2;
                }
            );
            //=================
            RegisterPacketWizard("ActorCast", "Switch to White Mage, and cast Glare",
                PacketDirection.Server,
                (packet, _) => packet.PacketSize == 64 && BitConverter.ToUInt16(packet.Data, (int)Offsets.IpcData) == 16533);
            //=================
            RegisterPacketWizard("Effect", "Wait for Glare caused damage",
                PacketDirection.Server,
                (packet, _) => packet.PacketSize == 156 && BitConverter.ToUInt16(packet.Data, (int)Offsets.IpcData + 8) == 16533);
            //=================
            RegisterPacketWizard("AddStatusEffect", "Please use Dia",
                PacketDirection.Server,
                (packet, _) => packet.PacketSize == 128 && BitConverter.ToUInt16(packet.Data, (int)Offsets.IpcData + 30) == 1871);
            //=================
            RegisterPacketWizard("StatusEffectList", "Please wait",
                PacketDirection.Server,
                (packet, _) => packet.PacketSize == 416 && BitConverter.ToUInt16(packet.Data, (int)Offsets.IpcData + 20) == 1871);
            //=================
            RegisterPacketWizard("ActorGauge", "Wait for gauge changes, then clear the lilies",
                PacketDirection.Server,
                (packet, _) => packet.PacketSize == 48 &&
                    packet.Data[(int)Offsets.IpcData] == 24 &&
                    packet.Data[(int)Offsets.IpcData + 5] == 0 &&
                    packet.Data[(int)Offsets.IpcData + 6] > 0);
            //=================
            RegisterPacketWizard("ActorControlTarget", "Place marker 'A' on the ground.",
                PacketDirection.Server,
                (packet, _) => packet.PacketSize == 48 &&
                    BitConverter.ToUInt16(packet.Data, (int)Offsets.IpcData) == 310 &&
                    BitConverter.ToUInt32(packet.Data, (int)Offsets.IpcData + 4) == 0);
            //=================
            RegisterPacketWizard("AoeEffect8", "Attack multiple enemies with Holy",
                PacketDirection.Server,
                (packet, _) => packet.PacketSize == 668 && BitConverter.ToUInt16(packet.Data, (int)Offsets.IpcData + 8) == 139);
            //=================
            RegisterPacketWizard("AoeEffect16", "Attack multiple enemies (>8) with Holy",
                PacketDirection.Server,
                (packet, _) => packet.PacketSize == 1244 && BitConverter.ToUInt16(packet.Data, (int)Offsets.IpcData + 8) == 139);
            //=================
            RegisterPacketWizard("AoeEffect24", "Attack multiple enemies (>16) with Holy",
                PacketDirection.Server,
                (packet, _) => packet.PacketSize == 1820 && BitConverter.ToUInt16(packet.Data, (int)Offsets.IpcData + 8) == 139);
            //=================
            RegisterPacketWizard("AoeEffect32", "Attack multiple enemies (>24) with Holy",
                PacketDirection.Server,
                (packet, _) => packet.PacketSize == 2396 && BitConverter.ToUInt16(packet.Data, (int)Offsets.IpcData + 8) == 139);

        }

        private void RegisterPacketWizard(string opName, string tutorial, PacketDirection scanDirection, Func<MetaPacket, string[], bool> del, int paramCount = 0)
        {
            wizards.Enqueue(new PacketWizard
            {
                OpName = opName,
                Tutorial = tutorial,
                PacketCheckerFunc = del,
                ParamCount = paramCount,
                ScanDirection = scanDirection
            });
        }

        public void Run(LinkedList<Packet> pq)
        {
            StringBuilder output = new StringBuilder();

            Console.WriteLine("The following packets are to be scanned:");
            for (var i = 0; i < wizards.Count; i++)
                Console.WriteLine($"#{i}: {wizards.ElementAt(i).OpName}");

            Console.WriteLine();
            
            // Game Version
            Console.WriteLine("Please enter the current game version: ");
            var versionNameFilter = new Regex(@"[^0-9.]", RegexOptions.Compiled);
            var gamePatch = versionNameFilter.Replace(Console.ReadLine(), (match) => "");

            Console.WriteLine("Press enter to run all wizards or the number of a wizard to skip to it.");
            var skipCount = Console.ReadLine();

            var count = 0;

            if (!string.IsNullOrEmpty(skipCount))
            {
                count = int.Parse(skipCount);

                for (var i = 0; i < count; i++)
                    wizards.Dequeue();
            }

            while (wizards.Count > 0)
            {
                var wizard = wizards.Dequeue();

                Console.WriteLine($"#{count}: Now scanning for {wizard.OpName}");

                if (!string.IsNullOrEmpty(wizard.Tutorial))
                    Console.WriteLine(wizard.Tutorial);

                var parameters = new string[wizard.ParamCount];
                if (wizard.ParamCount > 0)
                {
                    for (var paramIndex = 0; paramIndex < wizard.ParamCount; paramIndex++)
                    {
                        Console.WriteLine($"Please now enter parameter #{paramIndex}:");
                        var thisParam = Console.ReadLine();
                        parameters[paramIndex] = thisParam;
                    }
                }
                
                Console.WriteLine($"Scanning for {wizard.ScanDirection} packets... (Press Ctrl+C to skip)");

                var opCode = PacketScanner.Scan(pq, wizard.PacketCheckerFunc, parameters, wizard.ScanDirection, out bool cancelled);
                if (cancelled)
                {
                    Console.WriteLine($"{wizard.OpName} scanning skipped");
                }
                else
                {
                    Console.WriteLine($"{wizard.OpName} found at opcode 0x{opCode.ToString("X4")}!");
                    output.Append(wizard.OpName).Append(": 0x").Append(opCode.ToString("X4")).Append(", // updated ").AppendLine(gamePatch);
                }

                Console.WriteLine();
                count++;
            }

            // Done
            Console.WriteLine("All packets found!\n\n");
            Console.WriteLine(output.ToString());
            Console.ReadLine();
        }
    }
}
