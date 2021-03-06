using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

using Game.Managers;

namespace Game.Game_Objects.Trees
{
    class PineTreeStump : Tree
    {
        public PineTreeStump(ContentManager content)
        {
            base.Initialize(content, "models\\treeParts\\thinTreeStump2", null);
            base.ID = GameIDList.Prop_PineTreeStump;
        }

        public override void Draw()
        {
            base.trunk.Draw();
        }
    }
}
