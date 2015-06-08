using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlackholeBattle
{
    interface IMovable
    {
        Vector3 Position { get; set; }
        void Accelerate(Vector3 direction);
        void Brake();
    }
}
