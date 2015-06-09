using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace BlackholeBattle
{
    class Missile : GravitationalField
    {
        public Missile(string player, Vector3 startingPos, Vector3 directionVector, float size)
        {
            unitType = "missile";
            owner = player;
            bounds = new BoundingSphere(startingPos, size);
        }
        public Vector3 Rotation()
        {
            //find direction of missile
            return new Vector3((float)(Math.Tan(state.v.Y / state.v.Z) * 180 / Math.PI), (float)(Math.Tan(state.v.X / state.v.Z)* 180 / Math.PI), 0);
        }
        public void Update(List<GravitationalField> gravityObjects)
        {
            foreach (GravitationalField g in gravityObjects)
            {
                if (this != g)
                {
                    Vector3 distanceBetween = g.state.x - state.x;
                    float fG = (float)(G * g.mass / Math.Pow(distanceBetween.Length(), 2));
                    distanceBetween.Normalize();
                    netForce += distanceBetween * fG;
                }
            }
        }
    }
}
