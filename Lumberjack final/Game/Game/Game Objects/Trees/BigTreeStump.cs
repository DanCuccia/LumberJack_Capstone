using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

using Game.Managers;

namespace Game.Game_Objects.Trees
{
    class BigTreeStump : Tree
    {
        public BigTreeStump(ContentManager content)
        {
            base.Initialize(content, "models\\treeParts\\bigTreeStump", null);
            base.ID = GameIDList.Prop_BigStump;
        }

        public override void Draw()
        {
            base.trunk.Draw();
        }
    }
}
