using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BlackholeBattle
{
    abstract class Packet
    {
        protected byte PacketTypeID;
        public abstract void WritePacketData(List<byte> stream);
        public abstract void ReadPacketData(Stream stream);

        public static Packet ReadPacket(Stream stream)
        {
            var packetType = stream.ReadByte();
            Packet packet = null;
            switch (packetType)
            {
                case 1:
                    packet = new RequestConnectPacket();
                    packet.ReadPacketData(stream);
                    return packet;
                case 2:
                    packet = new ConnectDecisionPacket();
                    packet.ReadPacketData(stream);
                    return packet;
                default:
                    throw new Exception("Unrecognized Packet Type");
            }
        }

        public static void WritePacket(List<byte> stream, Packet packet)
        {
            stream.Add(packet.PacketTypeID);
            packet.WritePacketData(stream);
        }

        protected void WriteStringBytes(List<byte> stream, string str)
        {
            var numBytes = (short)ASCIIEncoding.ASCII.GetByteCount(str);

            //Un-necessary intermediate buffer.
            //TODO: Reduce memory usage and thus strain on the GC
            var arr = new byte[2 + numBytes];

            var lengthBytes = BitConverter.GetBytes(numBytes);
            if (BitConverter.IsLittleEndian)
                lengthBytes = lengthBytes.Reverse().ToArray();
            lengthBytes.CopyTo(arr, 0);

            var stringBytes = ASCIIEncoding.ASCII.GetBytes(str);
            stringBytes.CopyTo(arr, 2);

            stream.AddRange(arr);
        }

        protected string ReadStringFromStream(Stream stream)
        {
            var bytes = new byte[2];
            stream.Read(bytes, 0, 2);

            if (BitConverter.IsLittleEndian)
                bytes = bytes.Reverse().ToArray();
            var length = BitConverter.ToInt16(bytes, 0);

            var stringBytes = new byte[length];
            stream.Read(stringBytes, 0, length);

            return ASCIIEncoding.ASCII.GetString(stringBytes);
        }

        public byte GetPacketID()
        {
            return PacketTypeID;
        }
    }
}
