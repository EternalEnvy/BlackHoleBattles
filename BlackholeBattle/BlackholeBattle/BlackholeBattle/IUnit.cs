using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace BlackholeBattle
{
    interface IUnit
    {
        string UnitType();
        double Mass();
        Vector3 Position();
    }
}
