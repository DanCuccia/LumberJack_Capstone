using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework.Content;

using Game.Managers;

namespace Game.Game_Objects
{
    class Cabbage : LogicProp
    {
        public Cabbage(ContentManager content)
            : base()
        {
            base.model.Initialize(content, "models\\cabbage", "textures\\cabbage_diffuse");
            base.model.GenerateBoundingBox();
            base.ID = GameIDList.LogicProp_Cabbage;
        }
    }
}
