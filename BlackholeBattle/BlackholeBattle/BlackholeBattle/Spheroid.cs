using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace BlackholeBattle
{
    class Spheroid : GravitationalField
    {
        public virtual void Update(List<Tuple<Vector3, double>> positionMasses)
        {
            //find a vector representing the distance between two given masses, find the gravitational force, and divide to find the magnitude of acceleration.
            foreach (Tuple<Vector3, double> pm in positionMasses)
            {
                Vector3 distanceBetween = pm.Item1 - position;
                double fG = G * pm.Item2 / Math.Pow(distanceBetween.Length(), 2);
                distanceBetween.Normalize();
                acceleration += distanceBetween * (float)fG;
            }
            velocity += acceleration;
            position += velocity;
        }
        public virtual Vector3 Collide(double objectMass, Vector3 vec, Vector3 pos)
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


            velocity.X = (float)(2 * mass * velocity.X / (mass + objectMass) - (mass - objectMass) * vec.X / (mass + objectMass));
            velocity.Y = (float)(2 * mass * velocity.Y / (mass + objectMass) - (mass - objectMass) * vec.Y / (mass + objectMass));
            velocity.Z = (float)(2 * mass * velocity.Z / (mass + objectMass) - (mass - objectMass) * vec.Z / (mass + objectMass));

            velocity.Y -= 2 * (float)Math.Sin(pos.Y - position.Y);
            velocity.Z -= 2 * (float)Math.Sin(pos.Z - position.Z);
            velocity.X -= 2 * (float)Math.Sin(pos.X - position.X);

            velocity.X += (float)Math.Cos(pos.Y - position.Y);
            velocity.X += (float)Math.Cos(pos.Z - position.Z);

            velocity.Y += (float)Math.Cos(pos.X - position.X);
            velocity.Y += (float)Math.Cos(pos.Z - position.Z);

            velocity.Z += (float)Math.Cos(pos.X - position.X);
            velocity.Z += (float)Math.Cos(pos.Y - position.Y);

            return new Vector3(velocityX, velocityY, velocityZ);
        }
    }
}
