using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

using Game.Managers;

namespace Game.Game_Objects.Rocks
{
    class BoulderRock : Rock
    {
        public BoulderRock(ContentManager content)
        {
            if (Game1.runLowDef)
            {
                base.Initialize(content, "models\\rocks\\boulderRock", "textures\\rock_1_256");
            }
            else
            {
                base.Initialize(content, "models\\rocks\\boulderRock", "textures\\rock_1_512");
            }
            base.ID = GameIDList.Prop_BoulderRock;
        }
    }
}
