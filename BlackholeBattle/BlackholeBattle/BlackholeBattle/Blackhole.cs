using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlackholeBattle
{
    class Blackhole : GravitationalField, IUnit
    {
        string myOwner = "Yolo";
        public string Owner()
        {
            return myOwner;
        }
    }
}
