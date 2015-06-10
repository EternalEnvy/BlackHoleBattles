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
        public double orbitalPeriod = 5; //orbital period in seconds
        public Spheroid(Vector3 startingPos, Vector3 startingVelocity, double startingMass, double startingSize, double rotPeriod, string nameModel, string iowner)
        {
            owner = iowner;
            unitType = nameModel;
            bounds = new BoundingSphere(startingPos, (float)startingSize);
            orbitalPeriod = rotPeriod;
            size = startingSize;
            mass = startingMass;
            modelName = nameModel;
            state.x = startingPos;
            state.v = startingVelocity;
        }
        public void Update(List<GravitationalField> gravityObjects)
        {
            rotation = new Vector3(0, (float)(BlackholeBattle.elapsedTimeSeconds * 360 / orbitalPeriod), 0);
            //find a vector representing the distance between two given masses, find the gravitational force, and divide to find the magnitude of acceleration.
            foreach (GravitationalField g in gravityObjects)
            {
                if (this != g)
                {
                    Vector3 distanceBetween =  g.state.x - state.x;
                    if(g.size + size >= distanceBetween.Length())
                    {
                        Collide(g);
                    }
                    float fG = (float)(G * g.mass / Math.Pow(distanceBetween.Length(), 2));
                    distanceBetween.Normalize();
                    netForce += distanceBetween * fG;
                }
            }
            updatedInLoop = true;
        }
        public void Collide(GravitationalField gravityObject)
        {
            if (gravityObject is Blackhole)
            {
                BlackholeBattle.swallowedObjects.Add(this);
                gravityObject.mass += this.mass;
            }
            else
            {
                Vector3 objectVelocity;
                //http://farside.ph.utexas.edu/teaching/301/lectures/node76.html
                //given initial velocities and masses, find final velocity.
                if (gravityObject.updatedInLoop)
                    objectVelocity = gravityObject.preVelocity;
                else
                    objectVelocity = gravityObject.state.v;
                //find the component vector normal to the collision surface. Remove it from the velocity vector. Then reverse the component normal to the surface and add it back to the velocity vector
                Vector3 normal = gravityObject.state.x - state.x;
                normal.Normalize();
                Vector3 velocityHat = state.v;
                velocityHat.Normalize();
                Vector3 remainingVector = velocityHat - normal;
                remainingVector += -1 * (normal);
                remainingVector.Normalize();
                state.v = (float)((mass - gravityObject.mass) * state.v.Length() / (gravityObject.mass + mass) + (2 * gravityObject.mass * objectVelocity.Length()) / (mass + gravityObject.mass)) * remainingVector;
            }
        }
    }
}
