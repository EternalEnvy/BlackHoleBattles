using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace BlackholeBattle
{
    interface IUnit
    {
        string ModelName();
        double Mass();
        double Size();
        double Rotation();
        Vector3 Position();
    }
}
