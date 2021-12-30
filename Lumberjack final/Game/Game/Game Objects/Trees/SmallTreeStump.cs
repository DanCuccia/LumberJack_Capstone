using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

using Game.Managers;

namespace Game.Game_Objects.Trees
{
    class SmallTreeStump : Tree
    {
        public SmallTreeStump(ContentManager content)
        {
            base.Initialize(content, "models\\treeParts\\smallTreeStump", null);
            base.ID = GameIDList.Prop_SmallStump;
        }

        public override void Draw()
        {
            base.trunk.Draw();
        }
    }
}
