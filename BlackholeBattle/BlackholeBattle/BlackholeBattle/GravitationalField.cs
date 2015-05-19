using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace BlackholeBattle
{
    class GravitationalField : IMovable
    {
        public double size;
        public string modelName = "earth";
        protected const double G = 0.03;
        public double mass = 0;
        public Vector3 position { get; set; }
        public Vector3 velocity;
        public Vector3 acceleration;
        public Vector3 preVelocity;
        public bool updatedInLoop = false;
        public void Update()
        {
            preVelocity = velocity;
            velocity += acceleration;
            position += velocity;
        }
    }
    public struct State
    {
        public Vector3 x;
        public Vector3 v;
    }
    public struct Derivative
    {
        public Vector3 dx;
        public Vector3 dv;
    }
}
