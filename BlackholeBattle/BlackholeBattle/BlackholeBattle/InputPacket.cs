using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;

namespace BlackholeBattle
{
    class InputPacket : Packet
    {
        public long FrameNumber;
        public int SelectedBlackHoleID;
        public bool Front;
        public bool Back;
        public bool Left;
        public bool Right;
        public bool Up;
        public bool Down;
        public Vector3 CameraPosition;
        public Vector3 CameraRotation;

        public InputPacket()
        {
            PacketTypeID = 3;
        }

        public override void WritePacketData(List<byte> stream)
        {
            WriteLong(stream, FrameNumber);
            WriteInt(stream, SelectedBlackHoleID);
            var mask = (byte)((Front ? 1 << 5 : 0) |
                              (Back ? 1 << 4 : 0) |
                              (Left ? 1 << 3 : 0) |
                              (Right ? 1 << 2 : 0) |
                              (Up ? 1 << 1 : 0) |
                              (Down ? 1 : 0));
            stream.Add(mask);
            WriteVector3(stream, CameraPosition);
            WriteVector3(stream, CameraRotation);
        }

        public override void ReadPacketData(Stream stream)
        {
            FrameNumber = ReadLong(stream);
            SelectedBlackHoleID = ReadInt(stream);
            var mask = stream.ReadByte();
            Front = ((mask >> 5) & 1) == 1;
            Back = ((mask >> 4) & 1) == 1;
            Left = ((mask >> 3) & 1) == 1;
            Right = ((mask >> 2) & 1) == 1;
            Up = ((mask >> 1) & 1) == 1;
            Down = (mask & 1) == 1;
            CameraPosition  = ReadVector3(stream);
            CameraRotation = ReadVector3(stream);
        }
    }
}