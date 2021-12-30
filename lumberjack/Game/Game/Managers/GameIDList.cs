using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game.Managers
{
    /// <summary>DO NOT CHANGE PROP ID's -- FACTORIES AND XML I/O DEPEND ON THEM</summary>
    class GameIDList
    {
        public const int World = 0;
        public const int World_Node = 1;
        public const int Player = 2;

        public const int Trees_Begin = 99;
        public const int Prop_SmallBush = 100;
        public const int Prop_BigBush = 101;
        public const int Prop_SmallFullTree = 102;
        public const int Prop_LargeFullTree = 104;
        public const int Prop_SpikeyBush = 105;
        public const int Trees_End = 110;

        public const int Rocks_Begin = 150;
        public const int Prop_LongRock = 151;
        public const int Prop_BoulderRock = 152;
        public const int Rocks_End = 160;
    }
}
