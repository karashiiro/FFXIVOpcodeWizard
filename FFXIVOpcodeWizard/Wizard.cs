using Sapphire.Common.Network;
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace FFXIVOpcodeWizard
{
    static class Wizard
    {
        public static void Run(PacketQueue pq)
        {
            StringBuilder output = new StringBuilder();

            Console.WriteLine("Please enter the current game version: ");
            Regex versionNameFilter = new Regex(@"[^0-9.]");
            string gamePatch = versionNameFilter.Replace(Console.ReadLine(), (match) => "");

            Console.WriteLine("Scanning for ActorControl...");
            ushort actorControl = ScanActorControl(pq);
            Console.WriteLine("ActorControl found at opcode {0}!", actorControl.ToString("X4"));
            output.Append("ActorControl: 0x").Append(actorControl.ToString("X4")).Append(", // updated ").AppendLine(gamePatch);
        }
        

        /*
         * From here down are our helper methods to ID packets.
         */

        private static MetaPacket ScanGeneric(PacketQueue pq)
        {
            while (pq.Peek() == null)
            {
                Thread.Sleep(2);
            }

            Packet basePacket = pq.Pop();
            MetaPacket mp = new MetaPacket(basePacket)
            {
                PacketSize = BitConverter.ToUInt32(basePacket.Data, (int)Offsets.PacketSize),
                SegmentType = BitConverter.ToUInt16(basePacket.Data, (int)Offsets.SegmentType),
                Opcode = BitConverter.ToUInt16(basePacket.Data, (int)Offsets.IpcType)
            };
            return mp;
        }
        
        private static ushort ScanActorControl(PacketQueue pq)
        {
            MetaPacket foundPacket = null;
            while (foundPacket == null || foundPacket.PacketSize != 96)
            {
                foundPacket = ScanGeneric(pq);

                if (foundPacket.Direction == "outbound" ||
                    BitConverter.ToUInt16(foundPacket.Data, (int)Offsets.IpcData) != 23)
                {
                    foundPacket = null;
                }
            }
            return foundPacket.Opcode;
        }
    }
}
