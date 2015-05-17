﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlackholeBattle
{
    class Blackhole : GravitationalField, IUnit
    {
        string myOwner = "Yolo";
        public Vector3 position { get; set; }
        public string Owner()
        {
            return myOwner;
        }
    }
}
