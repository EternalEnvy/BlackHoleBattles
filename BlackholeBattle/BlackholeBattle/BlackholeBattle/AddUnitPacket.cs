using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlackholeBattle
{
    class AddUnitPacket : Packet
    {
        public AddUnitPacket(IUnit unit)
        {

        }
        public override void WritePacketData(List<byte> stream)
        {
            throw new NotImplementedException();
        }

        public override void ReadPacketData(System.IO.Stream stream)
        {
            throw new NotImplementedException();
        }
    }
}
