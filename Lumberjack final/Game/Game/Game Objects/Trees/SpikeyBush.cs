using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

using Game.Managers;

namespace Game.Game_Objects.Trees
{
    class SpikeyBush : Tree
    {
        public SpikeyBush(ContentManager content)
        {
            base.ID = GameIDList.Prop_SpikeyBush;
            base.Initialize(content, "models\\treeParts\\spikeyBushTrunk", "models\\treeParts\\spikeyBushTop");
        }
    }
}
