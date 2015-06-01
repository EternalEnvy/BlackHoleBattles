using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlackholeBattle
{
    class Player
    {
        public string name;
        public List<IUnit> myUnits = new List<IUnit>();
        public Player(string userName)
        {
            name = userName;
        }
        private void AddUnit(IUnit unit)
        {
            PacketQueue.Instance.AddPacket(new AddUnitPacket(unit));
            myUnits.Add(unit);
        }
    }
}
