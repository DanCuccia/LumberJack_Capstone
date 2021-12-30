using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

using Game.Managers;

namespace Game.Game_Objects.Trees
{
    class SmallFullTree : Tree
    {
        public SmallFullTree(ContentManager content)
        {
            base.Initialize(content, "models\\treeParts\\smallTreeTrunk", "models\\treeParts\\smallTreeTop");
            base.ID = GameIDList.Prop_SmallFullTree;
        }
    }
}
