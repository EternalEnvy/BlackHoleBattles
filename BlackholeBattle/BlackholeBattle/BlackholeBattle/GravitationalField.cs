using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace BlackholeBattle
{
    class GravitationalField
    {
        protected const double G = 100;
        public virtual double mass;
        public virtual Vector3 position;
        
    }
}
