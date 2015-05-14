using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlackholeBattle
{
    class Player
    {
        public string name;
        List<IUnit> myUnits = new List<IUnit>();
        public Player(string userName)
        {
            name = userName;
        }
    }
}
