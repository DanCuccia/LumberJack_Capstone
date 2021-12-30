using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

using Game.Managers;

namespace Game.Game_Objects.Trees
{
    class ThinTree : Tree
    {
        public ThinTree(ContentManager content)
        {
            base.Initialize(content, "models\\treeParts\\thinTreeTrunk", "models\\treeParts\\thinTreeTop");
            base.ID = GameIDList.Prop_ThinTree;
        }
    }
}
