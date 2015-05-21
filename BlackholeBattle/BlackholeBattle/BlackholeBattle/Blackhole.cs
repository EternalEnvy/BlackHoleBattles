using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlackholeBattle
{
    class Blackhole : GravitationalField, IUnit
    {
        string myOwner;
        public Blackhole(string player, double startMass, Vector3 startingPos)
        {
            mass = startMass;
            position = startingPos;
            velocity = Vector3.Zero;
            acceleration = Vector3.Zero;
            myOwner = player;
        }
    }
}
