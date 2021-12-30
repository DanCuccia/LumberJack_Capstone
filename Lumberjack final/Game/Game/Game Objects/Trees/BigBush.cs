using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

using Game.Managers;

namespace Game.Game_Objects.Trees
{
    class BigBush : Tree
    {
        public BigBush(ContentManager content)
        {
            base.Initialize(content, "models\\treeParts\\bigBushTrunk", "models\\treeParts\\bigBushTop");
            base.ID = GameIDList.Prop_BigBush;
        }
    }
}
