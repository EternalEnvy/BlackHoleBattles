using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace BlackholeBattle
{
    class Base : IUnit
    {
        bool shieldActive = false;
        double shieldRemain = 15;
        double cannonCooldown = 20;
        double blackHoleCreateCooldown = 45;
        string myOwner;
        Vector3 position;
        Vector3 velocity;
        Vector3 acceleration;
        public Base(string owner, Vector3 posit)
        {
            myOwner = owner;
            position = posit;
        }
        public string Owner()
        {
            return myOwner;
        }
        public double Mass()
        {
            return 0;
        }
        Vector3 IUnit.Position()
        {
            return position;
        }
        public string[] GetInfo()
        {
            return new string[]
            {
                Math.Ceiling(shieldRemain).ToString() + " seconds of shield remaining",
                Math.Ceiling(cannonCooldown).ToString() + "seconds of cannon cooldown",
                blackHoleCreateCooldown.ToString() + " seconds of blackhole creation cooldown",
            };
        }

        public void Update(double elapsedTime, Vector3 acceleration, bool toggleShield, bool shootLaser, Vector3 laserDirection, bool brake, bool createBlackhole)
        {
            
            if(shieldActive)
                shieldRemain -= elapsedTime;
            if(shieldRemain < 0)
                shieldRemain = 0;
            if (toggleShield)
            {
                if (shieldActive)
                    shieldActive = false;
                else
                    shieldActive = true;
            }
        }
    }
}
