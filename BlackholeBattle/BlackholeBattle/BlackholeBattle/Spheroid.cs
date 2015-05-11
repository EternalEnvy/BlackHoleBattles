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
        public Vector3 Collide(double objectMass, Vector3 vec)
        {
            //http://farside.ph.utexas.edu/teaching/301/lectures/node76.html
            //given initla velocities and masses, find final velocity. Modified to find velocity in x,y,and z direction
            return new Vector3((float)((mass - objectMass) * velocity.X / (mass + objectMass) + 2 * objectMass * vec.X / (mass + objectMass)), (float)((mass - objectMass) * velocity.Y / (mass + objectMass) + 2 * objectMass * vec.Y / (mass + objectMass)), (float)((mass - objectMass) * velocity.Z / (mass + objectMass) + 2 * objectMass * vec.Z / (mass + objectMass)));
        }
    }
}
