using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlackholeBattle
{
    class Blackhole : GravitationalField, IPlayerControlled
    {
        string myOwner;
        public string Owner()
        {
            return myOwner;
        }
        public Blackhole(string player, double startMass, Vector3 startingPos)
        {
            mass = startMass;
            state.x = startingPos;
            state.v = new Vector3(0, 0, 0);
            myOwner = player;
        }
    }
}
