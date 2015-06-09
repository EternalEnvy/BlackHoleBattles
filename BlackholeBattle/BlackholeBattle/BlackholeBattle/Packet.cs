using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using Microsoft.Xna.Framework;

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
                case 3:
                    packet = new InputPacket();
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

        protected long ReadLong(Stream stream)
        {
            var bytes = new byte[8];
            stream.Read(bytes, 0, 8);
            if (BitConverter.IsLittleEndian)
                bytes = bytes.Reverse().ToArray();
            return BitConverter.ToInt64(bytes, 0);
        }

        protected void WriteLong(List<byte> stream, long num)
        {
            var bytes = BitConverter.GetBytes(num);
            if (BitConverter.IsLittleEndian)
                bytes = bytes.Reverse().ToArray();
            stream.AddRange(bytes);
        }

        protected int ReadInt(Stream stream)
        {
            var bytes = new byte[4];
            stream.Read(bytes, 0, 4);
            if (BitConverter.IsLittleEndian)
                bytes = bytes.Reverse().ToArray();
            return BitConverter.ToInt32(bytes, 0);
        }

        protected void WriteInt(List<byte> stream, int num)
        {
            var bytes = BitConverter.GetBytes(num);
            if (BitConverter.IsLittleEndian)
                bytes = bytes.Reverse().ToArray();
            stream.AddRange(bytes);
        }

        protected long ReadShort(Stream stream)
        {
            var bytes = new byte[2];
            stream.Read(bytes, 0, 2);
            if (BitConverter.IsLittleEndian)
                bytes = bytes.Reverse().ToArray();
            return BitConverter.ToInt16(bytes, 0);
        }

        protected void WriteShort(List<byte> stream, short num)
        {
            var bytes = BitConverter.GetBytes(num);
            if (BitConverter.IsLittleEndian)
                bytes = bytes.Reverse().ToArray();
            stream.AddRange(bytes);
        }

        protected Vector3 ReadVector3(Stream stream)
        {
            var vec = new float[3];
            for (int i = 0; i < 3; i++)
            {
                var bytes = new byte[8];
                stream.Read(bytes, 0, 8);
                if (BitConverter.IsLittleEndian)
                    bytes = bytes.Reverse().ToArray();
                vec[i] = BitConverter.ToSingle(bytes, 0);
            }
            return new Vector3(vec[0], vec[1], vec[2]);
        }

        protected void WriteVector3(List<byte> stream, Vector3 num)
        {
            var vec = new[] {num.X, num.Y, num.Z};
            foreach(var n in vec)
            {
                var bytes = BitConverter.GetBytes(n);
                if (BitConverter.IsLittleEndian)
                    bytes = bytes.Reverse().ToArray();
                stream.AddRange(bytes);
            }
        }
        protected double ReadDouble(Stream stream)
        {
            var bytes = new byte[8];
            stream.Read(bytes, 0, 8);
            if (BitConverter.IsLittleEndian)
                bytes = bytes.Reverse().ToArray();
            return BitConverter.ToDouble(bytes, 0);
        }

        protected void WriteDouble(List<byte> stream, double num)
        {
            var bytes = BitConverter.GetBytes(num);
            if (BitConverter.IsLittleEndian)
                bytes = bytes.Reverse().ToArray();
            stream.AddRange(bytes);
        }

        public byte GetPacketID()
        {
            return PacketTypeID;
        }
    }
}
