using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

using Game.Managers;

namespace Game.Game_Objects.Trees
{
    class LargeFullTree : Tree
    {
        public LargeFullTree(ContentManager content)
        {
            base.Initialize(content, "models\\treeParts\\bigTreeTrunk", "models\\treeParts\\bigTreeTop");
            base.ID = GameIDList.Prop_LargeFullTree;
        }
    }
}
