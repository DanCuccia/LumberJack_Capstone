using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework.Content;

using Game.Managers;

namespace Game.Game_Objects
{
    public class BrokenHouse : LogicProp
    {
        public BrokenHouse(ContentManager content)
            :base()
        {
            base.model.Initialize(content, "models\\house", "textures\\house_diffuse");
            base.model.GenerateBoundingBox();
            base.ID = GameIDList.LogicProp_BrokenHouse;
        }
    }
}
