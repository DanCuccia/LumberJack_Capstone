using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework.Content;

using Game.Managers;

namespace Game.Game_Objects
{
    class RiverDam : LogicProp
    {
        public RiverDam(ContentManager content)
            :base()
        {
            base.model.Initialize(content, "models\\dam", "textures\\dam_diffuse");
            base.model.GenerateBoundingBox();
            base.ID = GameIDList.LogicProp_RiverDam;
        }
    }
}
