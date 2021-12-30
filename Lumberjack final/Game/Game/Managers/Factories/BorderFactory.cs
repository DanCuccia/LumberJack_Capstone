using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

using Game.Game_Objects;

namespace Game.Managers.Factories
{
    class BorderFactory : PropFactory
    {
        public BorderFactory() { }

        public override Prop getProp(ContentManager content, PropXMLStruct2 xmlInput)
        {
            if (xmlInput == null)
                return null;

            WorldBorder border = new WorldBorder(content);

            if (border != null)
            {
                border.Position = xmlInput.position;
                border.Rotation = xmlInput.rotation;
                border.Scale = xmlInput.scale;
            }

            return border as Prop;
        }
    }
}
