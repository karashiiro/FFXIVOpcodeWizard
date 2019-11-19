using System;
using System.Linq;

namespace FFXIVOpcodeWizard
{
    class PacketQueue
    {
        private Packet[] queue;
        private long curIndex;
        private long firstIndex;

        public PacketQueue()
        {
            queue = new Packet[1000];
            curIndex = 0;
            firstIndex = 0;
        }

        public void Push(Packet packet)
        {
            queue[curIndex] = packet;
            ++curIndex;
            if (curIndex == queue.Length)
            {
                curIndex = 0;
            }

            if (curIndex == firstIndex)
            {
                Packet[] newQueue = new Packet[queue.Length * 2];
                firstIndex = newQueue.Length - 1 - (queue.Length - firstIndex);
                Array.Copy(queue, 0, newQueue, firstIndex, queue.Length);
                queue = newQueue;
            }
        }

        public Packet Peek()
        {
            return queue[firstIndex];
        }

        public Packet Pop()
        {
            Packet returnable = queue[firstIndex];
            ++firstIndex;

            if (!BitConverter.IsLittleEndian)
            {
                returnable.Data = returnable.Data.Reverse().ToArray();
            }

            return returnable;
        }
    }
}
