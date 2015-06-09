using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace BlackholeBattle
{
    interface IUnit
    {
        string Owner();
        BoundingSphere GetBounds();
        string ModelName();
        double Mass();
        double Size();
        Vector3 Rotation();
        Vector3 Position();
    }
}
