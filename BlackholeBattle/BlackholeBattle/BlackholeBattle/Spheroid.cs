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
        public Spheroid(Vector3 startingPos, Vector3 startingVelocity, string nameModel)
        {
            modelName = nameModel;
            position = startingPos;
            velocity = startingVelocity;
            acceleration = new Vector3(0, 0, 0);
            mass = 1000;
        }
        public void Update(List<GravitationalField> gravityObjects)
        {
            //find a vector representing the distance between two given masses, find the gravitational force, and divide to find the magnitude of acceleration.
            foreach (GravitationalField g in gravityObjects)
            {
                if (this != g)
                {
                    Vector3 distanceBetween = g.position - position;
                    if(g.size + size > Math.Abs(distanceBetween.Length()))
                    {
                        Collide(g);
                    }
                    double fG = G * g.mass / Math.Pow(distanceBetween.Length(), 2);
                    distanceBetween.Normalize();
                    acceleration += distanceBetween * (float)fG;
                }
            }
            velocity += acceleration;
            position += velocity;
        }
        public void Collide(GravitationalField gravityObject)
        {
            //http://farside.ph.utexas.edu/teaching/301/lectures/node76.html
            //given initla velocities and masses, find final velocity. Modified to find velocity in x,y,and z direction
            float velocityX = (float)((mass - gravityObject.mass) * velocity.X / (mass + gravityObject.mass) + 2 * gravityObject.mass * gravityObject.velocity.X / (mass + gravityObject.mass));
            float velocityY = (float)((mass - gravityObject.mass) * velocity.Y / (mass + gravityObject.mass) + 2 * gravityObject.mass * gravityObject.velocity.Y / (mass + gravityObject.mass));
            float velocityZ = (float)((mass - gravityObject.mass) * velocity.Z / (mass + gravityObject.mass) + 2 * gravityObject.mass * gravityObject.velocity.Z / (mass + gravityObject.mass));

            //velocityY += 2 * (float)Math.Sin(pos.Y - position.Y);
            //velocityZ += 2 * (float)Math.Sin(pos.Z - position.Z);
            //velocityX += 2 * (float)Math.Sin(pos.X - position.X);

            //velocityX -= (float)Math.Cos(pos.Y - position.Y);
            //velocityX -= (float)Math.Cos(pos.Z - position.Z);

            //velocityY -= (float)Math.Cos(pos.X - position.X);
            //velocityY -= (float)Math.Cos(pos.Z - position.Z);

            //velocityZ -= (float)Math.Cos(pos.X - position.X);
            //velocityZ -= (float)Math.Cos(pos.Y - position.Y);


            velocity.X = (float)(2 * mass * velocity.X / (mass + gravityObject.mass) - (mass - gravityObject.mass) * gravityObject.velocity.X / (mass + gravityObject.mass));
            velocity.Y = (float)(2 * mass * velocity.Y / (mass + gravityObject.mass) - (mass - gravityObject.mass) * gravityObject.velocity.Y / (mass + gravityObject.mass));
            velocity.Z = (float)(2 * mass * velocity.Z / (mass + gravityObject.mass) - (mass - gravityObject.mass) * gravityObject.velocity.Z / (mass + gravityObject.mass));

            //velocity.Y -= 2 * (float)Math.Sin(pos.Y - position.Y);
            //velocity.Z -= 2 * (float)Math.Sin(pos.Z - position.Z);
            //velocity.X -= 2 * (float)Math.Sin(pos.X - position.X);

            //velocity.X += (float)Math.Cos(pos.Y - position.Y);
            //velocity.X += (float)Math.Cos(pos.Z - position.Z);

            //velocity.Y += (float)Math.Cos(pos.X - position.X);
            //velocity.Y += (float)Math.Cos(pos.Z - position.Z);

            //velocity.Z += (float)Math.Cos(pos.X - position.X);
            //velocity.Z += (float)Math.Cos(pos.Y - position.Y);

            gravityObject.velocity = new Vector3(velocityX, velocityY, velocityZ);
        }
    }
}
