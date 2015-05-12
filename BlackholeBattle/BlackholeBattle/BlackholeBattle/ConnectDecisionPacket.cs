using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BlackholeBattle
{
    class ConnectDecisionPacket : Packet
    {
        public bool Accepted;
        public ConnectDecisionPacket()
        {
            PacketTypeID = 2;
        }
        public override void WritePacketData(Stream stream)
        {
            stream.WriteByte(Accepted ? (byte)1 : (byte)0);
        }

        public override void ReadPacketData(Stream stream)
        {
            var b = stream.ReadByte();
            Accepted = b == 1;
        }
    }
}
