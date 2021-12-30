using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

using Game.Drawing_Objects;
using Game.Managers;
using Game.Math_Physics;

namespace Game.Game_Objects
{
    class Fence : LogicProp
    {
        public Fence(ContentManager content)
            :base()
        {
            base.model.Initialize(content, "models\\fence", RenderTechnique.RT_WOOD);
            base.model.GenerateBoundingBox();
            base.ID = GameIDList.LogicProp_Fence;
        }
    }
}
