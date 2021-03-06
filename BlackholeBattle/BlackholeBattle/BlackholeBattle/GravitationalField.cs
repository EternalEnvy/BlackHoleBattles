﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using Microsoft.Xna.Framework;

namespace BlackholeBattle
{
    public class GravitationalField : IUnit
    {
        protected Vector3 rotation = Vector3.Zero;
        protected string unitType = "Gravity Object";
        public int _id;
        private static int LastID = -1;
        public string ModelName()
        {
            return unitType;
        }   
        public BoundingSphere bounds = new BoundingSphere();
        public double size;
        protected string owner;
        public string modelName;
        protected const double G = 10;
        //Double uses 8 bytes, which will be sent for every object due to redundancy from UDP loss. Maybe a float would be better, or even a short? Do we need lots of precision?
        public double mass = 0;
        public State state;
        public Vector3 preVelocity { get; set; }
        public Vector3 netForce;
        public bool updatedInLoop = false;

        public string Owner()
        {
            return owner;
        }

        public GravitationalField()
        {
            _id = ++LastID;
        }
        public BoundingSphere GetBounds()
        {
            return bounds;
        }
        public double Mass()
        {
            return mass;
        }
        public Vector3 Position()
        {
            return state.x;
        }
        public Vector3 Rotation()
        {
            return rotation;
        }
        public double Size()
        {
            return size;
        }

        public int ID()
        {
            return _id;
        }
        public void Update()
        {
            preVelocity = state.v;
            if((state.x - Vector3.Zero).Length() > 10000)
            {
                state.v = -state.v;
            }
            state.v += netForce;
            state.x += state.v;
            bounds.Center = state.x;
            netForce = Vector3.Zero;
        }
        //RK4 very close to complete
        public void Update(float t, float dt)
        {
            preVelocity = state.v;
            Derivative a = Evaluate(state, t, 0.0f, new Derivative());
            Derivative b = Evaluate(state, t, dt * 0.5f, a);
            Derivative c = Evaluate(state, t, dt * 0.5f, b);
            Derivative d = Evaluate(state, t, dt, c);
            Vector3 dxdt = 1.0f / 6.0f * (a.dx + 2.0f * (b.dx + c.dx) + d.dx);
            Vector3 dvdt = 1.0f / 6.0f * (a.dv + 2.0f * (b.dv + c.dv) + d.dv);
            state.x += dxdt * dt;
            state.v += dvdt * dt;
            bounds.Center = state.x;
        }
        Derivative Evaluate(State initial, float t, float dt, Derivative d)
        {
            State newState;
            newState.x = initial.x + d.dx * dt; 
            newState.v = initial.v + d.dv * dt;
            Derivative der;
            der.dx = state.v;
            der.dv = Acceleration(state, t+dt);
            return der;
        }
        Vector3 Acceleration(State state, float t)
        {
            //ALL LOGIC HERE
            return state.v + netForce;
        }
    }
    public struct Derivative { 
        public Vector3 dx; // dx/dt = velocity 
        public Vector3 dv; // dv/dt = acceleration
    };
    public struct State 
    { 
        public Vector3 x; //position
        public Vector3 v; // velocity 
    };
}
