using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

using Game.Game_Objects;
using Game.Game_Objects.Rocks;

namespace Game.Managers.Factories
{
    class RockFactory : PropFactory
    {

        public RockFactory() { }

        public override Prop getProp(ContentManager content, PropXMLStruct xmlInput)
        {
            Rock rock = null;

            switch (xmlInput.id)
            {
                case GameIDList.Prop_LongRock:
                    rock = new LongRock(content);
                    break;
                case GameIDList.Prop_BoulderRock:
                    rock = new BoulderRock(content);
                    break;
            }

            if (rock != null)
            {
                rock.Position = xmlInput.position;
                rock.Rotation = xmlInput.rotation;
                rock.Scale = xmlInput.scale;
            }

            return rock as Prop;
        }
    }
}
