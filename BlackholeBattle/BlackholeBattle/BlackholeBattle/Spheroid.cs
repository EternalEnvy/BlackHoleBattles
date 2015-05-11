using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace BlackholeBattle
{
    class Spheroid
    {
        double mass;
        Vector3 position;
        Vector3 velocity;
        Vector3 acceleration;
        public Vector3 Collide(double objectMass, Vector3 vec, Vector3 pos)
        {
            //http://farside.ph.utexas.edu/teaching/301/lectures/node76.html
            //given initla velocities and masses, find final velocity. Modified to find velocity in x,y,and z direction
            float velocityX = (float)((mass - objectMass) * velocity.X / (mass + objectMass) + 2 * objectMass * vec.X / (mass + objectMass));
            float velocityY = (float)((mass - objectMass) * velocity.Y / (mass + objectMass) + 2 * objectMass * vec.Y / (mass + objectMass));
            float velocityZ = (float)((mass - objectMass) * velocity.Z / (mass + objectMass) + 2 * objectMass * vec.Z / (mass + objectMass));

            velocityY += 2 * (float)Math.Sin(pos.Y - position.Y);
            velocityZ += 2 * (float)Math.Sin(pos.Z - position.Z);
            velocityX += 2 * (float)Math.Sin(pos.X - position.X);

            velocityX -= (float)Math.Cos(pos.Y - position.Y);
            velocityX -= (float)Math.Cos(pos.Z - position.Z);

            velocityY -= (float)Math.Cos(pos.X - position.X);
            velocityY -= (float)Math.Cos(pos.Z - position.Z);

            velocityZ -= (float)Math.Cos(pos.X - position.X);
            velocityZ -= (float)Math.Cos(pos.Y - position.Y);
        }
    }
}
