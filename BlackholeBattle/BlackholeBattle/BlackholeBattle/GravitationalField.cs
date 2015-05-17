using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace BlackholeBattle
{
    class GravitationalField : IMovable
    {
        public double size = 200;
        public string modelName = "earth";
        protected static Random randall = new Random();
        protected const double G = 0.03;
        public double mass = 0;
        public Vector3 position { get; set; }
        public Vector3 velocity;
        public Vector3 acceleration;
        public void Update()
        {
            velocity += acceleration;
            position += velocity;
        }
    }
}
