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
        public abstract void WritePacketData(Stream stream);
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

        public static void WritePacket(Stream stream, Packet packet)
        {
            stream.WriteByte(packet.PacketTypeID);
            packet.WritePacketData(stream);
        }

        protected void WriteStringBytes(Stream stream, string str)
        {
            var numBytes = (short)ASCIIEncoding.ASCII.GetByteCount(str);

            var arr = new byte[2 + numBytes];

            var lengthBytes = BitConverter.GetBytes(numBytes);
            if (BitConverter.IsLittleEndian)
                lengthBytes = lengthBytes.Reverse().ToArray();
            lengthBytes.CopyTo(arr, 0);

            var stringBytes = ASCIIEncoding.ASCII.GetBytes(str);
            stringBytes.CopyTo(arr, 2);

            stream.Write(arr, 0, arr.Length);
        }

        protected string ReadStringFromStream(Stream stream)
        {
            var bytes = new byte[2];
            stream.Read(bytes, 0, 2);

            if (BitConverter.IsLittleEndian)
                bytes = bytes.Reverse().ToArray();
            var length = BitConverter.ToInt32(bytes, 0);

            var stringBytes = new byte[length];
            stream.Read(stringBytes, 0, length);

            return ASCIIEncoding.ASCII.GetString(stringBytes);
        }
    }
}
