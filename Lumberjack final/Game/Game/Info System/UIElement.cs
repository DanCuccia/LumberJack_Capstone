using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Game.Game_Objects;
using Game.Managers;

namespace Game
{
    public class UIElement : Infobox
    {
        Vector2 originalTarget;
        Vector2 hideTarget;

        public UIElement(InfoboxSetup ibs)
            : base( ibs )
        {
            originalTarget = targetPosition;
            hideTarget = position;
        }

        public override void update(GameTime time)
        {
        }

        
    }
}