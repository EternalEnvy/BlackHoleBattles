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
        public string ModelName()
        {
            return unitType;
        }
        bool shieldActive = false;
        double shieldRemain = 15000;
        double cannonCooldown = 20000;
        double blackHoleCreateCooldown = 45000;
        string myOwner;
        Vector3 position;
        Vector3 velocity;
        Vector3 acceleration;
        public Base(Vector3 posit, string modelName)
        {
            unitType = modelName;
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
            if (shieldActive)
                shieldRemain -= elapsedTime;
            blackHoleCreateCooldown -= elapsedTime;
            cannonCooldown -= elapsedTime;
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
        public double Size()
        {
            return 100;
        }
        public double Rotation()
        {
            return 0;
        }
        public void ShieldToggle()
        {
            shieldActive = !shieldActive;
        }
        public void Brake()
        {
            if (velocity.Length() < 0.1)
                velocity = Vector3.Zero;
            else
            {
                float length = velocity.Length();
                velocity.Normalize();
                velocity *= length - 0.1f;
            }
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
