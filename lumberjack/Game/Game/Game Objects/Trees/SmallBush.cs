using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

using Game.Managers;

namespace Game.Game_Objects.Trees
{
    class SmallBush : Tree
    {
        public SmallBush(ContentManager content)
        {
            base.Initialize(content, "models\\treeParts\\bushTrunk", "models\\treeParts\\bushTop");
            base.ID = GameIDList.Prop_SmallBush;
        }
    }
}
