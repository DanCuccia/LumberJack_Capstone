using System;
using System.Collections.Generic;

using Game.Game_Objects;

namespace Game.Managers
{
    /// <summary>DO NOT CHANGE PROP ID's -- FACTORIES AND XML I/O DEPEND ON THEM</summary>
    class GameIDList
    {
        #region ID LIST

        public const int World = 0;
        public const int World_Node = 1;
        public const int Player = 2;

        public const int Prop_Border = 98;


        public const int Trees_Begin = 99;
        public const int Prop_SmallBush = 100;
        public const int Prop_BigBush = 101;
        public const int Prop_SmallFullTree = 102;
        public const int Prop_LargeFullTree = 104;
        public const int Prop_SpikeyBush = 105;
        public const int Prop_ThinTree = 108;
        public const int Prop_PineTree = 109;
        public const int Trees_End = 120;

        public const int Stumps_Begin = 200;
        public const int Prop_BigStump = 201;
        public const int Prop_SmallStump = 202;
        public const int Prop_ThinTreeStump = 203;
        public const int Prop_PineTreeStump = 204;
        public const int Stumps_End = 210;

        public const int Rocks_Begin = 150;
        public const int Prop_LongRock = 151;
        public const int Prop_BoulderRock = 152;
        public const int Prop_CraigRock = 153;
        public const int Rocks_End = 160;

        public const int LogicProps_Begin = 300;
        public const int LogicProp_RiverDam = 301;
        public const int LogicProp_BrokenHouse = 302;
        public const int LogicProp_BoatDock = 303;
        public const int LogicProp_Fence = 304;
        public const int LogicProp_BoatA = 305;
        public const int LogicProp_BoatB = 306;
        public const int LogicProp_Cabbage = 307;
        public const int LogicProps_End = 320;

        #endregion ID LIST

        #region Static API

        public static bool IsLogicProp(int id)
        {
            if (id > GameIDList.LogicProps_Begin && id < GameIDList.LogicProps_End)
                return true;
            else return false;
        }

        public static bool IsTree(int id)
        {
            if (id > GameIDList.Trees_Begin && id < GameIDList.Trees_End)
                return true;
            else return false;
        }

        public static bool IsRock(int id)
        {
            if (id > GameIDList.Rocks_Begin && id < GameIDList.Rocks_End)
                return true;
            else return false;
        }

        public static bool IsBorder(int id)
        {
            if (id == Prop_Border)
                return true;
            else return false;
        }

        public static bool IsStump(int id)
        {
            if (id > GameIDList.Stumps_Begin && id < GameIDList.Stumps_End)
                return true;
            else return false;
        }

        #endregion Static API
    }
}
