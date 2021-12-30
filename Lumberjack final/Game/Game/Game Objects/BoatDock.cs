using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework.Content;

using Game.Managers;

namespace Game.Game_Objects
{
    class BoatDock : LogicProp
    {
        public BoatDock(ContentManager content)
            :base()
        {
            base.model.Initialize(content, "models\\dock", "textures\\dock_diffuse");
            base.model.GenerateBoundingBox();
            base.ID = GameIDList.LogicProp_BoatDock;
        }
    }
}
