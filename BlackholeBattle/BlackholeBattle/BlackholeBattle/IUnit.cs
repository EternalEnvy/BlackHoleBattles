using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace BlackholeBattle
{
    interface IUnit
    {
        double Mass();
        Vector3 Position();
    }
}
