using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BlackholeBattle
{
    class PacketQueue
    {
        List<Packet> _queue = new List<Packet>();
        long id = 0;
        long lastReceivedFromOther = -1;
        long lastReceivedFromMe = -1;

        public void AddPacket(Packet packet)
        {
            _queue.Add(packet);
        }

        private long ReadLong(Stream stream)
        {
            var bytes = new byte[4];
            stream.Read(bytes, 0, 2);
            if (BitConverter.IsLittleEndian)
                bytes = bytes.Reverse().ToArray();
            return BitConverter.ToInt64(bytes, 0);
        }

        private void WriteLong(Stream stream, long num)
        {
            var bytes = BitConverter.GetBytes(num);
            if (BitConverter.IsLittleEndian)
                bytes = bytes.Reverse().ToArray();
            stream.Write(bytes, 0, bytes.Length);
        }

        public Packet[] ReceivePackets(Stream stream)
        {
            //Read the most recent packet received in the current batch.
            var newLastReceivedFromOther = ReadLong(stream);

            //Get the number of new packets received in this call, this number may be negative, which is fine as the packets will be read and discarded.
            var numPackets2 = newLastReceivedFromOther - lastReceivedFromOther;

            //Read the most recent packet received from the other client.
            var newLastReceivedFromMe = ReadLong(stream);

            var amount = newLastReceivedFromMe - lastReceivedFromMe;
            if (amount > 0)
            {
                _queue.RemoveRange(0, (int)amount);
                lastReceivedFromMe = newLastReceivedFromMe;
            }

            //The number of packets received in this batch
            var numPackets = ReadLong(stream);

            //Only return new packets
            Packet[] packets = new Packet[numPackets2];
            for (var i = 0; i < numPackets; i++)
            {
                //If the packet is new, store it
                if (newLastReceivedFromOther - numPackets + i >= lastReceivedFromOther)
                    packets[newLastReceivedFromOther - numPackets + i - lastReceivedFromOther] = Packet.ReadPacket(stream);
                else
                    //We still have to read it if it's old, but we just don't use it.
                    Packet.ReadPacket(stream);
            }

            lastReceivedFromOther = Math.Max(newLastReceivedFromOther, newLastReceivedFromOther);

            return packets;
        }

        public void WritePackets(Stream stream)
        {
            WriteLong(stream, id);

            WriteLong(stream, lastReceivedFromOther);

            var numPackets = _queue.Count;

            WriteLong(stream, numPackets);

            for (int i = 0; i < numPackets; i++)
            {
                Packet.WritePacket(stream, _queue[i]);
            }
        }
    }
}
