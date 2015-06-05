using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace BlackholeBattle
{
    class Base : IUnit, IMovable
    {
        string unitType = "base";
        public string UnitType()
        {
            return unitType;
        }
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

        public void Update(double elapsedTime)
        {
            acceleration = Vector3.Zero;
            //cooldowns
        }
        public void Accelerate(Vector3 direction)
        {
            acceleration += direction * 0.2f;
            if(acceleration.Length() > 50)
            {
                acceleration.Normalize();
                acceleration *= 50;
            }
        }
        public void Brake()
        {
            Vector3 temp = velocity;
            temp.Normalize();
            velocity -= temp * 10;
        }
        public Vector3 Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
            }
        }
    }
}
