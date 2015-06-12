using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace BlackholeBattle
{
    class PacketQueue
    {
        public static PacketQueue Instance = new PacketQueue();
        List<Packet> _queue = new List<Packet>();
        long id = 0;
        long lastReceivedFromOther = -1;
        long lastReceivedFromMe = -1;

        public void AddPacket(Packet packet)
        {
            lock (lockobj)
                _queue.Add(packet);
            ++id;
        }

        private long ReadLong(Stream stream)
        {
            var bytes = new byte[8];
            stream.Read(bytes, 0, 8);
            if (BitConverter.IsLittleEndian)
                bytes = bytes.Reverse().ToArray();
            return BitConverter.ToInt64(bytes, 0);
        }

        private void WriteLong(List<byte> stream, long num)
        {
            var bytes = BitConverter.GetBytes(num);
            if (BitConverter.IsLittleEndian)
                bytes = bytes.Reverse().ToArray();
            stream.AddRange(bytes);
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
                lock(lockobj)
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

        public void WritePackets(List<byte> stream)
        {
            WriteLong(stream, id - 1);

            WriteLong(stream, lastReceivedFromOther);

            lock (lockobj)
            {
                var numPackets = _queue.Count;

                WriteLong(stream, numPackets);

                for (int i = 0; i < numPackets; i++)
                {
                    Packet.WritePacket(stream, _queue[i]);
                }
            }
        }

        static readonly object lockobj = new object();

        public void TestLoop(UdpClient client, IPEndPoint serverIP, Queue<Packet> queue, object queueLock)
        {
            var reset = serverIP.Address.Equals(IPAddress.Any);
            while (true)
            {
                var res = client.Receive(ref serverIP);
                BlackholeBattle.lastNetworkReceived = DateTime.Now;
                var otherIP = serverIP.Address;
                if (reset)
                    serverIP = new IPEndPoint(IPAddress.Any, serverIP.Port);
                var stream = new MemoryStream(res);
                var packets = ReceivePackets(stream);
                foreach (var packet in packets)
                {
                    lock (queueLock)
                        if (packet.GetPacketID() == 1)
                        {
                            var pac = (RequestConnectPacket) packet;
                            pac.IPAddress = string.Join(".", otherIP.GetAddressBytes().Select(a => a.ToString()));
                            queue.Enqueue(pac);
                        }
                        else
                        {
                            queue.Enqueue(packet);
                        }
                }
            }
        }

        public static void TestFunc(UdpClient client, IPEndPoint serverIP)
        {
            var buffer = new List<byte>();
            Instance.WritePackets(buffer);
            //Returns number of bytes sent. I'm assuming this is useless?
            client.Send(buffer.ToArray(), buffer.Count, serverIP);
        }
    }
}
