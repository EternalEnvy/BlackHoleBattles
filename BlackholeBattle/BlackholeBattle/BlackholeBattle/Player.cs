using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlackholeBattle
{
    class Player
    {
        public int playerID; //Player 1 or Player 2
        public string name;
        public Player(string userName)
        {
            name = userName;
        }
        private void AddUnit(IUnit unit)
        {
            //PacketQueue.Instance.AddPacket(new AddUnitPacket(unit));
            //myUnits.Add(unit);
        }
    }
}
