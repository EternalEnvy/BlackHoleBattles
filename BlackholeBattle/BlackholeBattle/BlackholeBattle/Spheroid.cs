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
            mass = 500;
        }
        public void Update(List<GravitationalField> gravityObjects)
        {
            //find a vector representing the distance between two given masses, find the gravitational force, and divide to find the magnitude of acceleration.
            foreach (GravitationalField g in gravityObjects)
            {
                if (this != g)
                {
                    Vector3 distanceBetween = g.position - position;
                    if(g.size + size > distanceBetween.Length())
                    {
                        Collide(g);
                    }
                    if (distanceBetween.Length() > 1000)
                        continue;
                    else
                    {
                        double fG = G * g.mass / Math.Pow(distanceBetween.Length(), 2);
                        distanceBetween.Normalize();
                        acceleration += distanceBetween * (float)fG;
                    }
                }
            }
            velocity += acceleration;
            position += velocity;
        }
        public void Collide(GravitationalField gravityObject)
        {
            float f = velocity.Length();
            float f2 = gravityObject.velocity.Length();
            if (velocity.Length() < 0.2 && gravityObject.velocity.Length() < 0.2)
            {
                velocity = Vector3.Zero;
                gravityObject.velocity = Vector3.Zero;
            }
            else
            {
                //http://farside.ph.utexas.edu/teaching/301/lectures/node76.html
                //given initial velocities and masses, find final velocity.
                Vector3 normal = position - gravityObject.position;
                normal.Normalize();
                Vector3 velocityHat = gravityObject.velocity;
                velocityHat.Normalize();
                Vector3 remainingVector = velocityHat - normal;
                remainingVector += (-1 * (normal));
                remainingVector.Normalize();
                gravityObject.velocity = (float)(2 * mass * velocity.Length() / (mass + gravityObject.mass) - (mass - gravityObject.mass) * gravityObject.velocity.Length() / (mass + gravityObject.mass)) * remainingVector;

                //the same to this object

                Vector3 normal2 = gravityObject.position - position;
                normal2.Normalize();
                Vector3 velocityHat2 = velocity;
                velocityHat2.Normalize();
                Vector3 remainingVector2 = velocityHat2 - normal2;
                remainingVector2 += -1 * (normal2);
                velocity = (float)((mass - gravityObject.mass) * velocity.Length() / (gravityObject.mass + mass) + (2 * gravityObject.mass * gravityObject.velocity.Length()) / (mass + gravityObject.mass)) * remainingVector2;
            }
        }
    }
}
