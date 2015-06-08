using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlackholeBattle
{
    class Blackhole : GravitationalField, IMovable
    {
        string myOwner;
        public string Owner()
        {
            return myOwner;
        }
        public Blackhole(string player, double startMass, Vector3 startingPos)
        {
            bounds = new BoundingSphere(startingPos, 30);
            unitType = "blackhole";
            mass = startMass;
            state.x = startingPos;
            state.v = new Vector3(0, 0, 0);
            myOwner = player;
        }
        public void Accelerate(Vector3 direction)
        {
            netForce += direction *(float)(5 / mass);

        }
        public void Brake()
        {
            Vector3 temp = state.v;
            temp.Normalize();
            state.v -= temp * 10;
        }
        public Vector3 Position
        {
            get
            {
                return state.x;
            }
            set
            {
                state.x = value;
            }
        }
    }
}
