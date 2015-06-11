using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
        BoundingSphere bounds;
        bool shieldActive = false;
        double shieldRemain = 15000;
        double cannonCooldown = 20000;
        double blackHoleCreateCooldown = 45000;
        string myOwner;
        Vector3 position;
        Vector3 velocity;
        Vector3 acceleration;
        private int _id;
        private static int LastID = -1;
        public Base(Vector3 posit, string modelName, string owner)
        {
            myOwner = owner;
            unitType = modelName;
            position = posit;
            bounds = new BoundingSphere(posit, 100);
            _id = ++LastID;
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
        public BoundingSphere GetBounds()
        {
            return bounds;
        }
        public void Update(double elapsedTime)
        {
            acceleration = Vector3.Zero;
            //cooldowns
            if (shieldActive)
                shieldRemain -= elapsedTime;
            blackHoleCreateCooldown -= elapsedTime;
            cannonCooldown -= elapsedTime;
            bounds.Center = velocity;
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
        public Vector3 Rotation()
        {
            return Vector3.Zero;
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

        public int ID()
        {
            return _id;
        }
    }
}
