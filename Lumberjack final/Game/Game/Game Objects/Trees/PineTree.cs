using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

using Game.Managers;

namespace Game.Game_Objects.Trees
{
    class PineTree : Tree
    {
        public PineTree(ContentManager content)
        {
            base.Initialize(content, "models\\treeParts\\pineTreeTrunk", "models\\treeParts\\pineTreeTop");
            base.ID = GameIDList.Prop_PineTree;
        }
    }
}
