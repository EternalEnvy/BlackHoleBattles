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
        public double orbitalPeriod = 0.5; //orbital period in seconds
        public Spheroid(Vector3 startingPos, Vector3 startingVelocity, double startingMass, double startingSize, string nameModel)
        {
            size = startingSize;
            mass = startingMass;
            modelName = nameModel;
            position = startingPos;
            velocity = startingVelocity;
            acceleration = new Vector3(0, 0, 0);
        }
        public void Update(List<GravitationalField> gravityObjects)
        {
            //find a vector representing the distance between two given masses, find the gravitational force, and divide to find the magnitude of acceleration.
            foreach (GravitationalField g in gravityObjects)
            {
                if (this != g)
                {
                    Vector3 distanceBetween = g.position - position;
                    if(g.size + size >= distanceBetween.Length())
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
            updatedInLoop = true;
        }
        public void Collide(GravitationalField gravityObject)
        {
                Vector3 objectVelocity;
                //http://farside.ph.utexas.edu/teaching/301/lectures/node76.html
                //given initial velocities and masses, find final velocity.
                if (gravityObject.updatedInLoop)
                    objectVelocity = gravityObject.preVelocity;                    
                else
                    objectVelocity = gravityObject.velocity;
                Vector3 normal = gravityObject.position - position;
                normal.Normalize();
                Vector3 velocityHat = velocity;
                velocityHat.Normalize();
                Vector3 remainingVector = velocityHat - normal;
                remainingVector += -1 * (normal);
                remainingVector.Normalize();
                velocity = (float)((mass - gravityObject.mass) * velocity.Length() / (gravityObject.mass + mass) + (2 * gravityObject.mass * objectVelocity.Length()) / (mass + gravityObject.mass)) * remainingVector;
        }
    }
}
