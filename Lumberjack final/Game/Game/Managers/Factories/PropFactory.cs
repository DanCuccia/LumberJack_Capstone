using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

using Game.Game_Objects;
using Game.Game_Objects.Trees;
using Game.Managers;

namespace Game.Managers.Factories
{
    abstract class PropFactory
    {
        public abstract Prop getProp(ContentManager content, PropXMLStruct2 xmlInput);
    }
}
