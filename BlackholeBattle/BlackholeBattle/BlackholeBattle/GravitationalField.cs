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
        public string modelName;
        protected const double G = 0.01;
        public double mass = 0;
        public Vector3 position { get; set; }
        public Vector3 velocity { get; set; }
        public Vector3 acceleration { get; set; }
        public Vector3 preVelocity { get; set; }
        public bool updatedInLoop = false;
        public void Update()
        {
            preVelocity = velocity;
            velocity += acceleration;
            position += velocity;
        }
        public void Update(float dt)
        {
            preVelocity = velocity;
            Tuple<Vector3, Vector3> a = Evaluate(0.0f, new Tuple<Vector3, Vector3>(Vector3.Zero, Vector3.Zero));
            Tuple<Vector3, Vector3> b = Evaluate(dt * 0.5f, a);
            Tuple<Vector3, Vector3> c = Evaluate(dt * 0.5f, b);
            Tuple<Vector3, Vector3> d = Evaluate(dt, c);
            Vector3 dxdt = 1.0f / 6.0f * (a.Item1 + 2.0f * (b.Item1 + c.Item1) + d.Item1);
            Vector3 dvdt = 1.0f / 6.0f * (a.Item2 + 2.0f * (b.Item2 + c.Item2) + d.Item2);
            position += dxdt * dt;
            velocity += velocity + dvdt * dt;
        }
        Tuple<Vector3, Vector3> Evaluate(float dt, Tuple<Vector3, Vector3> d)
        {
            Vector3 pos;
            Vector3 vel;
            pos = position + d.Item1 * dt; 
            vel = velocity + d.Item2 * dt;
            return new Tuple<Vector3, Vector3>(vel, Acceleration(pos, vel));
        }
        Vector3 Acceleration(Vector3 position, Vector3 velocity)
        {
            //ALL LOGIC HERE
            return velocity + acceleration;
        }
    }
}
