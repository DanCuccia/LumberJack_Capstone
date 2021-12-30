using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

using Game.Managers;

namespace Game.Game_Objects.Rocks
{
    class LongRock : Rock
    {
        public LongRock(ContentManager content)
        {
            if (Game1.runLowDef)
            {
                base.Initialize(content, "models\\rocks\\longRock", "textures\\rock_1_256");
            }
            else
            {
                base.Initialize(content, "models\\rocks\\longRock", "textures\\rock_1_512");
            }
            base.ID = GameIDList.Prop_LongRock;
        }
    }
}
