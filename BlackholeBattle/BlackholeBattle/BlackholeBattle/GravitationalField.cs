using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace BlackholeBattle
{
    class GravitationalField
    {
        protected static Random randall = new Random();
        protected const double G = 100;
        public double mass = 0;
        public Vector3 position;
        public Vector3 velocity;
        public Vector3 acceleration;
    }
}
