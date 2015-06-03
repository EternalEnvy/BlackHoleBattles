using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BlackholeBattle
{
    class RequestConnectPacket : Packet
    {
        public string Nickname;
        public string IPAddress;
        public RequestConnectPacket()
        {
            PacketTypeID = 1;
        }
        public override void WritePacketData(List<byte> stream)
        {
            WriteStringBytes(stream, Nickname);
        }

        public override void ReadPacketData(Stream stream)
        {
            Nickname = ReadStringFromStream(stream);
        }
    }
}